from pydantic import BaseModel, EmailStr
from typing import List, Optional
from datetime import datetime


# --- User Schemas ---
class UserCreate(BaseModel):
    name: str
    email: str
    password: str
    is_admin: bool = False
    contact_number: Optional[str] = None
    messenger_account: Optional[str] = None
    gcash_number: Optional[str] = None


class UserLogin(BaseModel):
    email: str
    password: str


class UserOut(BaseModel):
    id: int
    name: str
    email: str
    is_admin: bool
    contact_number: Optional[str] = None
    messenger_account: Optional[str] = None
    gcash_number: Optional[str] = None
    created_at: datetime

    class Config:
        from_attributes = True


class Token(BaseModel):
    access_token: str
    token_type: str
    user: UserOut


# --- Product Schemas ---
class ProductCreate(BaseModel):
    name: str
    description: Optional[str] = ""
    price: float
    category: str
    image_url: Optional[str] = ""
    stock: int


class ProductUpdate(BaseModel):
    name: Optional[str] = None
    description: Optional[str] = None
    price: Optional[float] = None
    category: Optional[str] = None
    image_url: Optional[str] = None
    stock: Optional[int] = None


class ProductOut(BaseModel):
    id: int
    name: str
    description: Optional[str]
    price: float
    category: str
    image_url: Optional[str]
    stock: int
    created_at: datetime

    class Config:
        from_attributes = True


# --- Order Schemas ---
class CartItem(BaseModel):
    product_id: int
    quantity: int


class OrderCreate(BaseModel):
    items: List[CartItem]
    payment_method: str  # "contact" or "gcash"
    payment_details: str


class OrderItemOut(BaseModel):
    id: int
    product_id: int
    product_name: str
    quantity: int
    unit_price: float

    class Config:
        from_attributes = True


class OrderOut(BaseModel):
    id: int
    order_number: str
    user_id: int
    total_amount: float
    status: str
    payment_method: Optional[str] = None
    payment_details: Optional[str] = None
    admin_approved: bool = False
    created_at: datetime
    items: List[OrderItemOut] = []
    user: Optional[UserOut] = None

    class Config:
        from_attributes = True
