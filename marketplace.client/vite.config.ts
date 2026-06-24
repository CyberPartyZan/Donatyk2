import { defineConfig, loadEnv } from "vite";
import react from "@vitejs/plugin-react-swc";
import { resolve } from "node:path";
import AutoImport from "unplugin-auto-import/vite";

const base = process.env.BASE_PATH || "/";
const isPreview = process.env.IS_PREVIEW ? true : false;

// https://vite.dev/config/
export default defineConfig(({ mode }) => {
    // loadEnv is required to read .env.development in vite.config.ts
    // process.env does NOT pick up Vite .env files automatically
    const env = loadEnv(mode, process.cwd(), "");
    const backend = env.VITE_BACKEND_URL || "https://127.0.0.1:7077";

    return {
        define: {
            __BASE_PATH__: JSON.stringify(base),
            __IS_PREVIEW__: JSON.stringify(isPreview),
            __READDY_PROJECT_ID__: JSON.stringify(env.PROJECT_ID || ""),
            __READDY_VERSION_ID__: JSON.stringify(env.VERSION_ID || ""),
            __READDY_AI_DOMAIN__: JSON.stringify(env.READDY_AI_DOMAIN || ""),
        },
        plugins: [
            react(),
            AutoImport({
                imports: [
                    {
                        react: [
                            "React",
                            "useState",
                            "useEffect",
                            "useContext",
                            "useReducer",
                            "useCallback",
                            "useMemo",
                            "useRef",
                            "useImperativeHandle",
                            "useLayoutEffect",
                            "useDebugValue",
                            "useDeferredValue",
                            "useId",
                            "useInsertionEffect",
                            "useSyncExternalStore",
                            "useTransition",
                            "startTransition",
                            "lazy",
                            "memo",
                            "forwardRef",
                            "createContext",
                            "createElement",
                            "cloneElement",
                            "isValidElement",
                        ],
                    },
                    {
                        "react-router-dom": [
                            "useNavigate",
                            "useLocation",
                            "useParams",
                            "useSearchParams",
                            "Link",
                            "NavLink",
                            "Navigate",
                            "Outlet",
                        ],
                    },
                    {
                        "react-i18next": ["useTranslation", "Trans"],
                    },
                ],
                dts: true,
            }),
        ],
        base,
        build: {
            sourcemap: true,
            outDir: "out",
        },
        resolve: {
            alias: {
                "@": resolve(__dirname, "./src"),
            },
        },
        server: {
            host: "127.0.0.1",
            port: 3000,
            strictPort: true,
            proxy: {
                "/api": {
                    target: backend,
                    changeOrigin: true,
                    secure: false,
                },
            },
        },
    };
});
