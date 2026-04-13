from fastapi import FastAPI
from fastapi.middleware.cors import CORSMiddleware
from fastapi.staticfiles import StaticFiles
from fastapi.responses import FileResponse, HTMLResponse
import os

from backend.database import engine, Base
from backend import models
from backend.routes import auth, products, orders
from backend.seed import seed_products, seed_admin


# Create all tables
Base.metadata.create_all(bind=engine)

# Seed default admin and sample products on first run
seed_admin()
seed_products()

app = FastAPI(title="Nexus Shop API", version="1.0.0")

# Allow frontend at same origin (and localhost dev)
app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

# API routes - MUST come before static file routes
app.include_router(auth.router)
app.include_router(products.router)
app.include_router(orders.router)

# Serve specific static files
@app.get("/style.css")
async def get_css():
    return FileResponse("style.css", media_type="text/css")

@app.get("/app.js")
async def get_js():
    return FileResponse("app.js", media_type="application/javascript")

# Root route to serve index.html
@app.get("/")
async def read_root():
    return FileResponse("index.html")
