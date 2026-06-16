import { BrowserRouter } from "react-router-dom";
import { AppRoutes } from "./router";
import { I18nextProvider } from "react-i18next";
import i18n from "./i18n";
import { AuthProvider } from "./hooks/useAuth";
import ChatWidget from "./components/feature/ChatWidget";


function App() {
    return (
        <I18nextProvider i18n={i18n}>
            <AuthProvider>
                <BrowserRouter basename={__BASE_PATH__}>
                    <AppRoutes />
                    <ChatWidget />
                </BrowserRouter>
            </AuthProvider>
        </I18nextProvider>
    );
}

export default App;
