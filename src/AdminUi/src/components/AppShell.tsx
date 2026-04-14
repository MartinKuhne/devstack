import { Outlet, Link } from "react-router-dom"
import { Moon, Sun, LayoutDashboard, Folder, AlertCircle, Settings } from "lucide-react"
import { Button } from "@/components/ui/button"
import { useEffect, useState } from "react"

function getInitialDarkMode() {
  if (typeof window !== "undefined") {
    const stored = localStorage.getItem("theme")
    if (stored) {
      return stored === "dark"
    }
    return window.matchMedia("(prefers-color-scheme: dark)").matches
  }
  return false
}

export function AppShell() {
  const [darkMode, setDarkMode] = useState(getInitialDarkMode)

  useEffect(() => {
    const root = window.document.documentElement
    if (darkMode) {
      root.classList.add("dark")
      localStorage.setItem("theme", "dark")
    } else {
      root.classList.remove("dark")
      localStorage.setItem("theme", "light")
    }
  }, [darkMode])

  useEffect(() => {
    const mediaQuery = window.matchMedia("(prefers-color-scheme: dark)")
    const handlechange = (e: MediaQueryListEvent) => {
      const stored = localStorage.getItem("theme")
      if (!stored) {
        setDarkMode(e.matches)
        const root = window.document.documentElement
        if (e.matches) {
          root.classList.add("dark")
        } else {
          root.classList.remove("dark")
        }
      }
    }
    mediaQuery.addEventListener("change", handlechange)
    return () => mediaQuery.removeEventListener("change", handlechange)
  }, [])

  return (
    <div className="min-h-screen bg-background text-foreground">
      <div className="flex min-h-screen w-full">
        <aside className="hidden w-64 border-r bg-background md:block">
          <div className="flex h-16 items-center border-b px-6">
            <h1 className="text-xl font-bold">DevStack</h1>
          </div>
          <nav className="p-4 space-y-2">
            <Link to="/">
              <Button variant="ghost" className="w-full justify-start">
                <LayoutDashboard className="mr-2 h-4 w-4" />
                Dashboard
              </Button>
            </Link>
            <Link to="/projects">
              <Button variant="ghost" className="w-full justify-start">
                <Folder className="mr-2 h-4 w-4" />
                Projects
              </Button>
            </Link>
            <Link to="/features">
              <Button variant="ghost" className="w-full justify-start">
                <Folder className="mr-2 h-4 w-4" />
                Features
              </Button>
            </Link>
            <Link to="/defects">
              <Button variant="ghost" className="w-full justify-start">
                <AlertCircle className="mr-2 h-4 w-4" />
                Defects
              </Button>
            </Link>
            <Link to="/settings">
              <Button variant="ghost" className="w-full justify-start">
                <Settings className="mr-2 h-4 w-4" />
                Settings
              </Button>
            </Link>
          </nav>
        </aside>
        <div className="flex-1 flex flex-col">
          <header className="flex h-16 items-center justify-between border-b px-6">
            <div className="flex items-center gap-4 md:hidden">
              <Button variant="ghost" size="icon">
                <LayoutDashboard className="h-5 w-5" />
              </Button>
            </div>
            <div className="flex items-center gap-2">
              <Button
                variant="ghost"
                size="icon"
                onClick={() => setDarkMode(!darkMode)}
              >
                {darkMode ? (
                  <Sun className="h-5 w-5" />
                ) : (
                  <Moon className="h-5 w-5" />
                )}
              </Button>
            </div>
          </header>
          <main className="flex-1 p-6">
            <Outlet />
          </main>
        </div>
      </div>
    </div>
  )
}
