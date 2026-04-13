from backend.database import SessionLocal
from backend import models
from backend.auth import hash_password


SAMPLE_PRODUCTS = [
    {
        "name": "Neon Genesis Headset",
        "description": "Premium wireless headphones with spatial audio and active noise cancellation.",
        "price": 16799.00,
        "category": "Electronics",
        "image_url": "https://images.unsplash.com/photo-1505740420928-5e560c06d30e?w=500&q=80",
        "stock": 15,
    },
    {
        "name": "Cyberpunk Jacket",
        "description": "Waterproof illuminated street jacket with embedded LED strips.",
        "price": 8372.00,
        "category": "Apparel",
        "image_url": "https://images.unsplash.com/photo-1551028719-00167b16eac5?w=500&q=80",
        "stock": 8,
    },
    {
        "name": "UI Kit Pro",
        "description": "Complete Figma design system for modern apps. 400+ components.",
        "price": 3304.00,
        "category": "Digital",
        "image_url": "https://images.unsplash.com/photo-1561070791-2526d30994b5?w=500&q=80",
        "stock": 999,
    },
    {
        "name": "Mechanical Keyboard",
        "description": "RGB mechanical keyboard with custom tactile switches and aluminum body.",
        "price": 10080.00,
        "category": "Electronics",
        "image_url": "https://images.unsplash.com/photo-1595225476474-87563907a212?w=500&q=80",
        "stock": 5,
    },
    {
        "name": "Minimal Watch",
        "description": "Scandinavian-inspired minimal watch with sapphire glass and leather strap.",
        "price": 12320.00,
        "category": "Apparel",
        "image_url": "https://images.unsplash.com/photo-1523275335684-37898b6baf30?w=500&q=80",
        "stock": 12,
    },
    {
        "name": "Code Snippets Pack",
        "description": "500+ ready-to-use code snippets for web and mobile developers.",
        "price": 1679.00,
        "category": "Digital",
        "image_url": "https://images.unsplash.com/photo-1517694712202-14dd9538aa97?w=500&q=80",
        "stock": 999,
    },
]


def seed_admin():
    """Create default admin account if it doesn't exist"""
    db = SessionLocal()
    try:
        admin_email = "admin@nexus.com"
        existing_admin = db.query(models.User).filter(models.User.email == admin_email).first()
        if not existing_admin:
            admin = models.User(
                name="Admin User",
                email=admin_email,
                hashed_password=hash_password("admin123"),
                is_admin=True,
                contact_number="09123456789",
                messenger_account="admin.nexus",
                gcash_number="09123456789"
            )
            db.add(admin)
            db.commit()
            print(f"[OK] Default admin created: {admin_email} / admin123")
        else:
            print(f"[INFO] Admin already exists, skipping seed.")

        # Sample customer account
        customer_email = "juan@email.com"
        existing_customer = db.query(models.User).filter(models.User.email == customer_email).first()
        if not existing_customer:
            customer = models.User(
                name="Juan Dela Cruz",
                email=customer_email,
                hashed_password=hash_password("customer123"),
                is_admin=False,
                contact_number="09987654321",
                messenger_account="juan.delacruz",
                gcash_number="09987654321"
            )
            db.add(customer)
            db.commit()
            print(f"[OK] Sample customer created: {customer_email} / customer123")
        else:
            print(f"[INFO] Sample customer already exists, skipping seed.")
    finally:
        db.close()


def seed_products():
    db = SessionLocal()
    try:
        count = db.query(models.Product).count()
        if count == 0:
            for p in SAMPLE_PRODUCTS:
                db.add(models.Product(**p))
            db.commit()
            print(f"[OK] Seeded {len(SAMPLE_PRODUCTS)} sample products.")
        else:
            print(f"[INFO] Products already exist ({count}), skipping seed.")
    finally:
        db.close()
