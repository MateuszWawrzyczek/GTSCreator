import React from "react";
import { Outlet, useLocation } from "react-router-dom";
import Navbar from "./Navbar";
import Lines from "./Lines";
import LineStops from "./LineStops";
//import Stops from "./Stops"; // nowy komponent
import "bootstrap/dist/css/bootstrap.min.css";

const AppLayout = () => {
  const location = useLocation();

  // wybór komponentu dla panelu bocznego
  const renderSidebar = () => {
    if (location.pathname.startsWith("/linie")) {
      return <Lines />;
    }
    if (location.pathname.startsWith("/route")) {
      return <LineStops />; // <-- dodajesz tu
    }
    
    return <div>Wybierz opcję z menu</div>;
  };

  return (
    <div className="d-flex flex-column min-vh-100">
      <Navbar />

      <div className="container-fluid flex-grow-1">
        <div className="row h-100">
          {/* Lewy panel */}
          <aside className="col-12 col-md-4 col-lg-3 border-end p-3 overflow-auto">
            {renderSidebar()}
          </aside>

          {/* Prawa część (Outlet = prawa kolumna) */}
          <main className="col-12 col-md-8 col-lg-9 p-3">
            <Outlet />
          </main>
        </div>
      </div>
    </div>
  );
};

export default AppLayout;
