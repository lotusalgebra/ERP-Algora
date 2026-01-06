// Algora Shop - Frontend JavaScript

// Cart functionality
const Shop = {
    // Get cart from localStorage
    getCart: function() {
        return JSON.parse(localStorage.getItem('cart') || '[]');
    },

    // Save cart to localStorage
    saveCart: function(cart) {
        localStorage.setItem('cart', JSON.stringify(cart));
        this.updateCartCount();
    },

    // Update cart count badge
    updateCartCount: function() {
        const cart = this.getCart();
        const count = cart.reduce((sum, item) => sum + item.quantity, 0);
        const countEl = document.getElementById('cartCount');
        if (countEl) {
            countEl.textContent = count;
        }
    },

    // Add item to cart
    addToCart: function(productId, name, price, image, quantity = 1) {
        const cart = this.getCart();
        const existing = cart.find(item => item.productId === productId);

        if (existing) {
            existing.quantity += quantity;
        } else {
            cart.push({ productId, name, price, image, quantity });
        }

        this.saveCart(cart);
        this.showToast('Added to Cart', `${name} has been added to your cart.`);
    },

    // Remove item from cart
    removeFromCart: function(productId) {
        let cart = this.getCart();
        cart = cart.filter(item => item.productId !== productId);
        this.saveCart(cart);
    },

    // Update item quantity
    updateQuantity: function(productId, quantity) {
        const cart = this.getCart();
        const item = cart.find(item => item.productId === productId);

        if (item) {
            item.quantity = Math.max(0, quantity);
            if (item.quantity === 0) {
                this.removeFromCart(productId);
            } else {
                this.saveCart(cart);
            }
        }
    },

    // Get cart total
    getTotal: function() {
        const cart = this.getCart();
        return cart.reduce((sum, item) => sum + (item.price * item.quantity), 0);
    },

    // Clear cart
    clearCart: function() {
        localStorage.removeItem('cart');
        localStorage.removeItem('couponCode');
        localStorage.removeItem('couponDiscount');
        this.updateCartCount();
    },

    // Show toast notification
    showToast: function(title, message) {
        const toastEl = document.getElementById('cartToast');
        if (toastEl) {
            const titleEl = toastEl.querySelector('.me-auto');
            const bodyEl = toastEl.querySelector('.toast-body');

            if (titleEl) titleEl.textContent = title;
            if (bodyEl) {
                bodyEl.innerHTML = `${message}<div class="mt-2 pt-2 border-top"><a href="/Shop/Cart" class="btn btn-primary btn-sm">View Cart</a></div>`;
            }

            const toast = new bootstrap.Toast(toastEl);
            toast.show();
        }
    },

    // Initialize
    init: function() {
        this.updateCartCount();

        // Handle quick add buttons
        document.querySelectorAll('[data-add-to-cart]').forEach(btn => {
            btn.addEventListener('click', (e) => {
                e.preventDefault();
                const productId = btn.dataset.productId;
                const name = btn.dataset.productName;
                const price = parseFloat(btn.dataset.productPrice);
                const image = btn.dataset.productImage;

                this.addToCart(productId, name, price, image);
            });
        });
    }
};

// Initialize on DOM ready
document.addEventListener('DOMContentLoaded', () => Shop.init());

// Export for global access
window.Shop = Shop;
