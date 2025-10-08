// Custom Swagger UI with Role Selector
(function() {
    'use strict';

    const roleCredentials = {
        admin: { email: 'admin@affiliate.com', password: 'Admin@123', label: 'Admin' },
        manager: { email: 'manager@affiliate.com', password: 'Manager@123', label: 'Manager' },
        customer: { email: 'customer1@affiliate.com', password: 'Customer@123', label: 'Customer' }
    };

    // Wait for Swagger UI to load
    window.addEventListener('load', function() {
        setTimeout(initRoleSelector, 1000);
    });

    function initRoleSelector() {
        // Create role selector container
        const container = document.createElement('div');
        container.id = 'role-selector-container';
        container.innerHTML = `
            <div style="position: fixed; top: 10px; right: 20px; z-index: 9999; background: white; padding: 15px; border-radius: 8px; box-shadow: 0 2px 8px rgba(0,0,0,0.15); border: 1px solid #e0e0e0;">
                <div style="margin-bottom: 10px;">
                    <label style="font-weight: bold; margin-right: 10px; font-size: 14px;">🚀 Quick Login:</label>
                    <select id="roleSelect" style="padding: 6px 10px; font-size: 14px; border: 1px solid #ccc; border-radius: 4px; margin-right: 10px;">
                        <option value="">Select Role</option>
                        <option value="admin">👑 Admin</option>
                        <option value="manager">👨‍💼 Manager</option>
                        <option value="customer">👤 Customer</option>
                    </select>
                    <button id="loginBtn" style="padding: 6px 16px; background: #4990e2; color: white; border: none; border-radius: 4px; cursor: pointer; font-size: 14px; font-weight: 500;">
                        Login & Authorize
                    </button>
                </div>
                <div id="roleInfo" style="font-size: 12px; color: #666; margin-top: 8px;"></div>
            </div>
        `;

        // Insert at the beginning of body
        document.body.insertBefore(container, document.body.firstChild);

        // Add event listener
        document.getElementById('loginBtn').addEventListener('click', quickLogin);
    }

    async function quickLogin() {
        const roleSelect = document.getElementById('roleSelect');
        const roleInfoEl = document.getElementById('roleInfo');
        const role = roleSelect.value;

        if (!role) {
            roleInfoEl.innerHTML = '<span style="color: #e74c3c;">⚠️ Please select a role</span>';
            return;
        }

        const credentials = roleCredentials[role];
        roleInfoEl.innerHTML = '<span style="color: #3498db;">⏳ Logging in...</span>';

        try {
            const baseUrl = window.location.origin;

            // Login request
            const response = await fetch(`${baseUrl}/api/auth/login`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({
                    email: credentials.email,
                    password: credentials.password
                })
            });

            if (!response.ok) {
                const errorData = await response.json();
                throw new Error(errorData.message || 'Login failed');
            }

            const data = await response.json();
            const token = data.data.token;

            // Find and click authorize button
            const authorizeBtn = document.querySelector('.authorize');
            if (authorizeBtn) {
                authorizeBtn.click();

                // Wait for modal to open
                setTimeout(() => {
                    const tokenInput = document.querySelector('input[type="text"][placeholder*="Bearer"]');
                    if (tokenInput) {
                        tokenInput.value = `Bearer ${token}`;

                        // Click authorize in modal
                        const modalAuthorizeBtn = document.querySelector('.auth-btn-wrapper .authorize');
                        if (modalAuthorizeBtn) {
                            modalAuthorizeBtn.click();
                        }

                        // Close modal
                        setTimeout(() => {
                            const closeBtn = document.querySelector('.close-modal');
                            if (closeBtn) closeBtn.click();
                        }, 500);
                    }
                }, 500);
            }

            roleInfoEl.innerHTML = `<span style="color: #27ae60;">✅ Logged in as <strong>${credentials.label}</strong></span>`;

        } catch (error) {
            roleInfoEl.innerHTML = `<span style="color: #e74c3c;">❌ ${error.message}</span>`;
            console.error('Login error:', error);
        }
    }
})();
