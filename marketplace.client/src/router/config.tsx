import type { RouteObject } from "react-router-dom";
import NotFound from "../pages/NotFound";
import Home from "../pages/home/page";
import Marketplace from "../pages/marketplace/page";
import ItemDetails from "../pages/item-details/page";
import Orders from "../pages/orders/page";
import AdminPanel from "../pages/admin/page";
import CategoriesAdmin from "../pages/admin/categories/page";
import LotsAdmin from "../pages/admin/lots/page";
import SellerInfo from "../pages/admin/seller/page";
import SellersAdmin from "../pages/admin/sellers/page";
import UsersAdmin from "../pages/admin/users/page";
import GoalsAdmin from "../pages/admin/goals/page";
import ShipmentsAdmin from "../pages/admin/shipments/page";
import GoalManagementAdmin from "../pages/admin/goal-management/page";
import CompensationsAdmin from "../pages/admin/compensations/page";
import AccountInfo from "../pages/account/page";
import LoginPage from "../pages/login/page";
import ForgotPasswordPage from "../pages/forgot-password/page";
import ResetPasswordPage from "../pages/reset-password/page";
import ConfirmEmailPage from "../pages/confirm-email/page";
import PaymentSuccessPage from "../pages/payment-success/page";
import PaymentCancelPage from "../pages/payment-cancel/page";
import CheckoutPage from "../pages/checkout/page";
import SellerPage from "../pages/marketplace/seller/page";
import GoalDetailPage from "../pages/goals/page";
import ReportsAdmin from "../pages/admin/report/page";
import MediaAdmin from "../pages/admin/media/page";
import FaqsAdmin from "../pages/admin/faqs/page";

const routes: RouteObject[] = [
    {
        path: "/",
        element: <Home />,
    },
    {
        path: "/marketplace",
        element: <Marketplace />,
    },
    {
        path: "/marketplace/item/:id",
        element: <ItemDetails />,
    },
    {
        path: "/marketplace/seller/:id",
        element: <SellerPage />,
    },
    {
        path: "/goals/:id",
        element: <GoalDetailPage />,
    },
    {
        path: "/orders",
        element: <Orders />,
    },
    {
        path: "/login",
        element: <LoginPage />,
    },
    {
        path: "/forgot-password",
        element: <ForgotPasswordPage />,
    },
    {
        path: "/reset-password",
        element: <ResetPasswordPage />,
    },
    {
        path: "/confirm-email",
        element: <ConfirmEmailPage />,
    },
    {
        path: "/checkout",
        element: <CheckoutPage />,
    },
    {
        path: "/payment/success",
        element: <PaymentSuccessPage />,
    },
    {
        path: "/payment/cancel",
        element: <PaymentCancelPage />,
    },
    {
        path: "/admin",
        element: <AdminPanel />,
        children: [
            {
                path: "categories",
                element: <CategoriesAdmin />,
            },
            {
                path: "lots",
                element: <LotsAdmin />,
            },
            {
                path: "seller",
                element: <SellerInfo />,
            },
            {
                path: "sellers",
                element: <SellersAdmin />,
            },
            {
                path: "users",
                element: <UsersAdmin />,
            },
            {
                path: "goals",
                element: <GoalsAdmin />,
            },
            {
                path: "shipments",
                element: <ShipmentsAdmin />,
            },
            {
                path: "goal-management",
                element: <GoalManagementAdmin />,
            },
            {
                path: "compensations",
                element: <CompensationsAdmin />,
            },
            {
                path: "reports",
                element: <ReportsAdmin />,
            },
            {
                path: "media",
                element: <MediaAdmin />,
            },
            {
                path: "faqs",
                element: <FaqsAdmin />,
            },
            {
                path: "account",
                element: <AccountInfo />,
            },
        ],
    },
    {
        path: "*",
        element: <NotFound />,
    },
];

export default routes;