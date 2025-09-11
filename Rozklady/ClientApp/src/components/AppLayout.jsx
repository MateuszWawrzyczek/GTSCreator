import React from "react";
import { Outlet, useLocation } from "react-router-dom";
import Navbar from "./Navbar";
import Lines from "./Lines";
import LineStops from "./LineStops";
import Stops from "./Stops";
import StopDepartures from "./StopDepartures";
import "bootstrap/dist/css/bootstrap.min.css";

const AppLayout = () => {
  const location = useLocation();

  const renderSidebar = () => {
    if (location.pathname.startsWith("/linie")) {
      return <Lines />;
    }
    if (location.pathname.startsWith("/route")) {
      return <LineStops />; 
    }

    if (location.pathname.startsWith("/stops")) {
      return <Stops />; 
    }
    if (location.pathname.startsWith("/stop/")) {
      return <StopDepartures />; 
    }
    
    return <div>Wybierz opcję z menu</div>;
  };

    return (
    <div className="d-flex flex-column min-vh-100">
      <Navbar className="sticky-top" />
      <div className="flex-grow-1 h-100">
        {renderSidebar()}
      </div>
    </div>
  );
};

export default AppLayout;
