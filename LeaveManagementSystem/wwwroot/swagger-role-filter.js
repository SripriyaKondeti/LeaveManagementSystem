(function () {
    "use strict";

    console.log("Swagger role filter loaded.");

    /*
     * ============================================
     * MANAGER-ONLY ROUTES
     * ============================================
     */
    const managerOnlyRoutes = new Set([
        "GET:/api/dashboard/leave-summary",

        "GET:/api/employees",
        "POST:/api/employees",
        "DELETE:/api/employees/{id}",

        "GET:/api/leavetypes",
        "GET:/api/leavetypes/{id}",
        "POST:/api/leavetypes",
        "PUT:/api/leavetypes/{id}",
        "DELETE:/api/leavetypes/{id}",

        "PUT:/api/leaves/{id}/approve",
        "PUT:/api/leaves/{id}/reject"
    ]);


    /*
     * ============================================
     * SHARED ROUTES
     * ============================================
     */
    const sharedRoutes = new Set([
        "GET:/api/employees/{id}",
        "PUT:/api/employees/{id}",

        "GET:/api/employees/{employeeId}/leave-balance",

        "POST:/api/leaves",
        "GET:/api/leaves",
        "GET:/api/leaves/{id}",
        "GET:/api/employees/{employeeId}/leaves"
    ]);


    /*
     * ============================================
     * PUBLIC ROUTES
     * ============================================
     */
    const publicRoutes = new Set([
        "POST:/api/auth/login"
    ]);


    /*
     * ============================================
     * DECODE JWT
     * ============================================
     */
    function decodeJwt(token) {
        try {
            const parts = token.split(".");

            if (parts.length !== 3) {
                return null;
            }

            let payload = parts[1];

            payload = payload
                .replace(/-/g, "+")
                .replace(/_/g, "/");

            while (payload.length % 4 !== 0) {
                payload += "=";
            }

            return JSON.parse(atob(payload));
        }
        catch (error) {
            console.error(
                "Unable to decode JWT:",
                error
            );

            return null;
        }
    }


    /*
     * ============================================
     * GET ROLE FROM JWT
     * ============================================
     */
    function getRoleFromJwt(token) {

        const payload =
            decodeJwt(token);

        if (!payload) {
            return null;
        }

        return (
            payload.role ||
            payload[
            "http://schemas.microsoft.com/ws/2008/06/identity/claims/role"
            ] ||
            null
        );
    }


    /*
     * ============================================
     * GET CURRENT JWT FROM SWAGGER UI
     * ============================================
     */
    function getCurrentJwt() {

        try {

            if (!window.ui) {
                return null;
            }

            const authState =
                window.ui
                    .getState()
                    .get("auth");

            if (!authState) {
                return null;
            }

            const authorized =
                authState.get("authorized");

            if (
                !authorized ||
                authorized.size === 0
            ) {
                return null;
            }

            const authorizedObject =
                authorized.toJS();

            const bearer =
                authorizedObject.Bearer ||
                authorizedObject.bearer;

            if (!bearer) {
                return null;
            }

            let token = null;

            if (
                typeof bearer === "string"
            ) {
                token = bearer;
            }
            else if (
                bearer.value
            ) {
                token = bearer.value;
            }

            if (!token) {
                return null;
            }

            return token
                .replace(/^Bearer\s+/i, "")
                .trim();

        }
        catch (error) {

            console.error(
                "Unable to get Swagger authorization:",
                error
            );

            return null;
        }
    }


    /*
     * ============================================
     * GET METHOD + PATH
     * ============================================
     */
    function getOperationKey(operation) {

        const pathElement =
            operation.querySelector(
                ".opblock-summary-path"
            );

        const methodElement =
            operation.querySelector(
                ".opblock-summary-method"
            );

        if (
            !pathElement ||
            !methodElement
        ) {
            return null;
        }

        const method =
            methodElement.textContent
                .trim()
                .toUpperCase();

        const path =
            pathElement.textContent
                .trim()
                .replace(/\s+/g, "")
                .replace(
                    /\{([^}]+)\}/g,
                    "{$1}"
                );

        return `${method}:${path}`;
    }


    /*
     * ============================================
     * UPDATE VISIBILITY
     * ============================================
     */
    function updateVisibility() {

        const token =
            getCurrentJwt();


        /*
         * ========================================
         * BEFORE LOGIN
         *
         * ONLY PUBLIC ROUTES
         * ========================================
         */
        if (!token) {

            console.log(
                "Swagger: No authentication - showing public routes only."
            );

            showOnlyPublicRoutes();

            return;
        }


        /*
         * ========================================
         * GET USER ROLE
         * ========================================
         */
        const role =
            getRoleFromJwt(token);

        console.log(
            "Swagger detected role:",
            role
        );


        /*
         * If role cannot be determined,
         * show only public routes.
         */
        if (!role) {

            console.warn(
                "Swagger: Role not found."
            );

            showOnlyPublicRoutes();

            return;
        }


        /*
         * ========================================
         * MANAGER
         * ========================================
         */
        if (
            role.toLowerCase() ===
            "manager"
        ) {

            showManagerRoutes();

            return;
        }


        /*
         * ========================================
         * EMPLOYEE
         * ========================================
         */
        if (
            role.toLowerCase() ===
            "employee"
        ) {

            showEmployeeRoutes();

            return;
        }


        /*
         * Unknown role
         */
        console.warn(
            "Swagger: Unknown role:",
            role
        );

        showOnlyPublicRoutes();
    }


    /*
     * ============================================
     * BEFORE LOGIN
     *
     * SHOW ONLY PUBLIC ROUTES
     * ============================================
     */
    function showOnlyPublicRoutes() {

        document
            .querySelectorAll(".opblock")
            .forEach(operation => {

                const key =
                    getOperationKey(operation);

                if (
                    key &&
                    publicRoutes.has(key)
                ) {

                    operation.style.display =
                        "";

                }
                else {

                    operation.style.display =
                        "none";
                }
            });

        removeEmptyTags();
    }


    /*
     * ============================================
     * MANAGER
     *
     * SHOW:
     * Public
     * Shared
     * Manager-only
     * ============================================
     */
    function showManagerRoutes() {

        document
            .querySelectorAll(".opblock")
            .forEach(operation => {

                const key =
                    getOperationKey(operation);

                if (
                    publicRoutes.has(key) ||
                    sharedRoutes.has(key) ||
                    managerOnlyRoutes.has(key)
                ) {

                    operation.style.display =
                        "";

                }
                else {

                    operation.style.display =
                        "none";
                }
            });

        removeEmptyTags();
    }


    /*
     * ============================================
     * EMPLOYEE
     *
     * SHOW:
     * Public
     * Shared
     *
     * HIDE:
     * Manager-only
     * ============================================
     */
    function showEmployeeRoutes() {

        document
            .querySelectorAll(".opblock")
            .forEach(operation => {

                const key =
                    getOperationKey(operation);

                if (
                    publicRoutes.has(key) ||
                    sharedRoutes.has(key)
                ) {

                    operation.style.display =
                        "";

                }
                else {

                    operation.style.display =
                        "none";
                }
            });

        removeEmptyTags();
    }


    /*
     * ============================================
     * REMOVE EMPTY TAGS
     * ============================================
     */
    function removeEmptyTags() {

        document
            .querySelectorAll(".opblock-tag")
            .forEach(tag => {

                const operations =
                    tag.querySelectorAll(
                        ".opblock"
                    );

                if (
                    operations.length === 0
                ) {
                    return;
                }

                const visibleOperations =
                    Array.from(
                        operations
                    ).filter(
                        operation =>
                            operation.style
                                .display !==
                            "none"
                    );

                tag.style.display =
                    visibleOperations.length > 0
                        ? ""
                        : "none";
            });
    }


    /*
     * ============================================
     * OBSERVE SWAGGER UI
     * ============================================
     */
    const observer =
        new MutationObserver(() => {

            updateVisibility();

        });


    function startObserver() {

        if (!document.body) {
            return;
        }

        observer.observe(
            document.body,
            {
                childList: true,
                subtree: true
            }
        );

        updateVisibility();
    }


    if (
        document.readyState ===
        "loading"
    ) {

        document.addEventListener(
            "DOMContentLoaded",
            startObserver
        );

    }
    else {

        startObserver();

    }


    /*
     * Check for login/logout changes.
     */
    setInterval(
        updateVisibility,
        1000
    );

})();