import React from "react";
import { useLocation } from "react-router-dom";
import Navbar from "./Navbar";
import Lines from "./Lines";
import LineStops from "./LineStops";
import Stops from "./Stops";
import StopDepartures from "./StopDepartures";
import "bootstrap/dist/css/bootstrap.min.css";
import TripStops from "./TripStops";
import VehiclesList from "./VehiclesList";
import BrigadesList from "./Brigades";

const AppLayout = () => {
  const location = useLocation();

  const renderSidebar = () => {
    if (location.pathname === "/"){
      return <Lines />;
    }
    if (location.pathname.startsWith("/linie")) {
      return <Lines />;
    }
    if (location.pathname.startsWith("/route")) {
      return <LineStops />; 
    }

    if (location.pathname.startsWith("/przystanki")) {
      return <Stops />; 
    }
    if (location.pathname.startsWith("/stop/")) {
      return <StopDepartures />; 
    }
    if (location.pathname.startsWith("/trip/")) {
      return <TripStops />; 
    }
    if (location.pathname.startsWith("/lista-pojazdow")) {
      return <VehiclesList />;
    }
    if (location.pathname === "/brygady") {
      return <BrigadesList />;
    }
    return <Stops />;
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
