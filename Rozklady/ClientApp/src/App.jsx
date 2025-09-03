import React from "react";
import { Routes, Route, Navigate  } from "react-router-dom";
import AppLayout from "./components/AppLayout";
import LineStops from "./components/LineStops";
//import HomePage from "./pages/HomePage";
//import Lines from "./components/Lines";
//import Stops from "./components/Stops"; // np. nowy komponent na przystanki
import "./custom.css";

function App() {
  return (
    <Routes>
      <Route path="/" element={<AppLayout />}>
        {/* Domyślnie przekieruj na /linie */}
        <Route index element={<Navigate to="/linie" replace />} />
        <Route path="route/:feedId/:routeId" element={<div></div>} />


        {/* Rozkład linii */}
        <Route path="linie" element={<div>Mapa/treść dla linii</div>} />


        {/* Przystanki 
        {<Route path="przystanki" element={<div>Mapa/treść dla przystanków</div>} />}*/}

        {/* Fallback dla nieznanych adresów */}
        <Route path="*" element={<Navigate to="/linie" replace />} />
      </Route>
    </Routes>
  );
}

export default App;
