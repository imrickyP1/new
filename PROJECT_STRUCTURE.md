# Nexus Shop - Project Structure

## 📁 Root Directory
```
webyou/
├── 📄 index.html           # Main HTML template (all views in one file)
├── 📄 style.css            # All CSS styles and animations
├── 📄 app.js               # Frontend JavaScript (router, auth, shop, cart, admin)
├── 📄 main.py              # FastAPI entry point and server configuration
├── 📄 requirements.txt     # Python dependencies
├── 📄 shop.db              # SQLite database (auto-created)
├── 📄 PROJECT_STRUCTURE.md # This file
└── 📁 backend/             # Backend Python modules
```

---

## 🎨 Frontend Files

### **index.html** - Single Page Application
- Navigation bar with cart and user menu
- Template sections for all views:
  - `#view-home` - Hero section and featured products
  - `#view-shop` - Product catalog with filters
  - `#view-account` - User profile and order history
  - `#view-admin` - Admin dashboard (products & orders)
- Modals:
  - Auth modal (login/register)
  - Cart sidebar
  - Checkout payment info modal
- Toast notification container

### **style.css** - Complete Styling
- CSS custom properties (theme colors)
- Global styles and animations
- Component styles:
  - Navigation, cards, buttons, forms
  - Modal overlays and sidebars
  - Tables, tabs, cart items
  - Responsive layouts (mobile-first)

### **app.js** - Frontend Logic (~600 lines)
Organized into modules:
```javascript
// Core utilities
- api()           // HTTP request helper
- formatMoney()   // Currency formatter (₱)
- toast()         // Notification system

// Modules
app.router        // SPA navigation
app.auth          // Login, register, session management
app.shop          // Product display, filters, search
app.cart          // Cart management (client-side)
app.transactions  // Order history and details
app.admin         // Admin CRUD for products & orders
```

---

## 🐍 Backend Structure

```
backend/
├── __init__.py          # Empty (marks as Python package)
├── database.py          # SQLAlchemy setup and session management
├── models.py            # Database models (User, Product, Order, OrderItem)
├── schemas.py           # Pydantic schemas for validation
├── auth.py              # JWT auth, password hashing, dependencies
├── seed.py              # Database seeding (admin user + sample products)
└── routes/              # API route handlers
    ├── __init__.py      # Empty
    ├── auth.py          # POST /register, /login
    ├── products.py      # GET/POST/PUT/DELETE /products
    └── orders.py        # POST /orders, GET /orders/user/{id}
```

### **Backend File Details**

#### **database.py**
- SQLite connection string
- SQLAlchemy engine and SessionLocal
- `Base` for model inheritance
- `get_db()` dependency for route handlers

#### **models.py**
- `User` - Authentication and profile
- `Product` - Shop inventory
- `Order` - Customer orders
- `OrderItem` - Order line items (many-to-many)

#### **schemas.py**
Pydantic models for request/response validation:
- UserCreate, UserLogin, UserOut
- ProductCreate, ProductUpdate, ProductOut
- OrderCreate, OrderItemCreate, OrderOut

#### **auth.py**
- `hash_password()` - bcrypt hashing
- `verify_password()` - password verification
- `create_access_token()` - JWT token generation
- `require_current_user()` - Auth dependency
- `require_admin()` - Admin-only dependency

#### **seed.py**
- `seed_admin()` - Creates default admin account
  - Email: admin@nexus.com
  - Password: admin123
- `seed_products()` - Populates 6 sample products

#### **routes/auth.py**
- `POST /api/auth/register` - Create new user
- `POST /api/auth/login` - Authenticate and get token

#### **routes/products.py**
- `GET /api/products` - List products (with optional category/search filters)
- `POST /api/products` - Create product (admin only)
- `PUT /api/products/{id}` - Update product (admin only)
- `DELETE /api/products/{id}` - Delete product (admin only)

#### **routes/orders.py**
- `POST /api/orders` - Create new order (authenticated users)
- `GET /api/orders/user/{user_id}` - Get user's order history

---

## 🚀 Quick Reference

### File Sizes (Approximate)
| File | Lines | Purpose |
|------|-------|---------|
| index.html | ~340 | All HTML templates |
| style.css | ~550 | All styles |
| app.js | ~600 | All frontend logic |
| main.py | ~35 | Server setup |
| backend/models.py | ~70 | Database models |
| backend/routes/*.py | ~150 | API endpoints |

### Key Functions by Location

**Need to modify prices/currency?**
→ `app.js` - `formatMoney()` function

**Need to add/modify products?**
→ `backend/seed.py` - `SAMPLE_PRODUCTS` array
→ Or use admin dashboard in browser

**Need to change admin credentials?**
→ `backend/seed.py` - `seed_admin()` function

**Need to add new API endpoint?**
→ Create route in `backend/routes/` and include in `main.py`

**Need to modify database schema?**
→ `backend/models.py` - Add/modify SQLAlchemy models
→ Then delete `shop.db` and restart server

**Need to change UI/styling?**
→ `index.html` - HTML structure
→ `style.css` - Styles and animations
→ `app.js` - Dynamic rendering logic

---

## 🔧 Running the Project

1. **Install dependencies:**
   ```bash
   pip install -r requirements.txt
   ```

2. **Start server:**
   ```bash
   python -m uvicorn main:app --reload
   ```

3. **Access application:**
   - Frontend: http://localhost:8000
   - API docs: http://localhost:8000/docs

4. **Default admin login:**
   - Email: admin@nexus.com
   - Password: admin123

---

## 📊 Data Flow

```
Browser (index.html)
    ↓
JavaScript (app.js)
    ↓
API Request → main.py
    ↓
Route Handler (backend/routes/*.py)
    ↓
Database Model (backend/models.py)
    ↓
SQLite (shop.db)
```

---

## 🎯 Common Tasks

### Add a new product category
1. No code changes needed!
2. Just create products with new category name
3. Frontend automatically shows all categories

### Modify payment methods
1. Edit `index.html` - checkout modal payment options
2. Update `app.js` - `app.cart.confirmCheckout()` validation

### Add user fields
1. `backend/models.py` - Add column to User model
2. `backend/schemas.py` - Add field to schemas
3. `index.html` - Add input field to registration form
4. `app.js` - Update register() to send new field

### Change email/password validation
1. `backend/schemas.py` - Modify Pydantic validators
2. `app.js` - Add client-side validation

---

## 📝 Notes

- **Single Page App:** All views are in index.html, routing handled by app.js
- **No build step:** Pure HTML/CSS/JS, no webpack/npm needed
- **Auto-reload:** Server restarts automatically when code changes
- **Database:** SQLite file created automatically on first run
- **Authentication:** JWT tokens stored in localStorage
- **Admin access:** Controlled by `is_admin` boolean in User model
