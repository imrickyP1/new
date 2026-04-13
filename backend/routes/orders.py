from fastapi import APIRouter, Depends, HTTPException
from sqlalchemy.orm import Session
from typing import List
import random, string
from .. import models, schemas
from ..database import get_db
from ..auth import require_current_user, require_admin

router = APIRouter(prefix="/api/orders", tags=["orders"])


def generate_order_number():
    return "ORD-" + "".join(random.choices(string.digits, k=6))


@router.post("", response_model=schemas.OrderOut)
def create_order(
    order_in: schemas.OrderCreate,
    db: Session = Depends(get_db),
    current_user=Depends(require_current_user)
):
    if not order_in.items:
        raise HTTPException(status_code=400, detail="Cart is empty")

    total = 0.0
    order_items = []

    for cart_item in order_in.items:
        product = db.query(models.Product).filter(
            models.Product.id == cart_item.product_id
        ).first()

        if not product:
            raise HTTPException(status_code=404, detail=f"Product {cart_item.product_id} not found")
        if product.stock < cart_item.quantity:
            raise HTTPException(
                status_code=400,
                detail=f"Insufficient stock for '{product.name}'. Available: {product.stock}"
            )

        item_total = product.price * cart_item.quantity
        total += item_total

        order_items.append({
            "product": product,
            "product_name": product.name,
            "quantity": cart_item.quantity,
            "unit_price": product.price,
            "product_id": product.id
        })

    # Deduct stock and build order
    order = models.Order(
        order_number=generate_order_number(),
        user_id=current_user.id,
        total_amount=round(total, 2),
        status="Pending",
        payment_method=order_in.payment_method,
        payment_details=order_in.payment_details,
        admin_approved=False
    )
    db.add(order)
    db.flush()  # Get order.id without committing

    for item_data in order_items:
        product = item_data.pop("product")
        product.stock -= item_data["quantity"]
        order_item = models.OrderItem(order_id=order.id, **item_data)
        db.add(order_item)

    db.commit()
    db.refresh(order)
    return order


@router.get("/my", response_model=List[schemas.OrderOut])
def get_my_orders(
    db: Session = Depends(get_db),
    current_user=Depends(require_current_user)
):
    return db.query(models.Order).filter(
        models.Order.user_id == current_user.id
    ).order_by(models.Order.created_at.desc()).all()


@router.get("", response_model=List[schemas.OrderOut])
def get_all_orders(
    db: Session = Depends(get_db),
    admin=Depends(require_admin)
):
    return db.query(models.Order).order_by(
        models.Order.created_at.desc()
    ).all()


@router.put("/{order_id}/approve", response_model=schemas.OrderOut)
def approve_order(
    order_id: int,
    db: Session = Depends(get_db),
    admin=Depends(require_admin)
):
    order = db.query(models.Order).filter(models.Order.id == order_id).first()
    if not order:
        raise HTTPException(status_code=404, detail="Order not found")
    
    order.admin_approved = True
    order.status = "Approved"
    db.commit()
    db.refresh(order)
    return order


@router.get("/customers", response_model=List[schemas.UserOut])
def get_customers(
    db: Session = Depends(get_db),
    admin=Depends(require_admin)
):
    """Get all customers who have placed orders"""
    customers = db.query(models.User).join(models.Order).distinct().all()
    return customers
