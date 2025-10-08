import { useEffect, useState } from "react";
import { MapContainer, TileLayer, Marker, Popup } from "react-leaflet";
import L from "leaflet";
import { Link } from "react-router-dom";
import "leaflet/dist/leaflet.css";
import feedColors from "../styles/feedColors.js";

const busStopIcon = new L.Icon({
  iconUrl: process.env.PUBLIC_URL + "/icons/bus-stop.svg",
  iconSize: [24, 24],
  iconAnchor: [16, 32],
  popupAnchor: [0, -32],
});

const busIcon = new L.Icon({
  iconUrl: process.env.PUBLIC_URL + "/icons/bus.png",
  iconSize: [28, 28],
  iconAnchor: [14, 28],
  popupAnchor: [0, -28],
});

function StopsMap({ stops = [], vehicles = [] }) {
  const [selectedStop, setSelectedStop] = useState(null);
  const [routes, setRoutes] = useState([]);

  const defaultCenter = stops.length
    ? [
        (Math.min(...stops.map((s) => s.stopLat)) + Math.max(...stops.map((s) => s.stopLat))) / 2,
        (Math.min(...stops.map((s) => s.stopLon)) + Math.max(...stops.map((s) => s.stopLon))) / 2,
      ]
    : [50.025, 18.54];

  useEffect(() => {
    if (!selectedStop) return;

    const fetchRoutes = async () => {
      try {
        const url = `https://localhost:7002/api/stops/${selectedStop.feedId}/${selectedStop.stopId}/routes`;
        const res = await fetch(url, { mode: "cors" });
        if (!res.ok) throw new Error("Błąd pobierania linii");
        const data = await res.json();
        setRoutes(data);
      } catch (err) {
        console.error(err);
        setRoutes([]);
      }
    };

    fetchRoutes();
  }, [selectedStop]);

  return (
    <MapContainer
      center={defaultCenter}
      zoom={12}
      style={{ height: "100%", width: "100%" }}
    >
      <TileLayer
        url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png"
        attribution='&copy; <a href="https://www.openstreetmap.org/">OpenStreetMap</a> contributors'
      />

      {stops.map((stop) => (
        <Marker
          key={`${stop.feedId}_${stop.stopId}`}
          position={[stop.stopLat, stop.stopLon]}
          icon={busStopIcon}
          eventHandlers={{
            click: () => setSelectedStop(stop),
          }}
        >
          {selectedStop &&
            selectedStop.stopId === stop.stopId &&
            selectedStop.feedId === stop.feedId && (
              <Popup>
                <div style={{ minWidth: "180px" }}>
                  <strong>{stop.stopName}</strong>
                  <br />
                  <Link to={`/stop/${stop.feedId}/${stop.stopId}`}>
                    Rozkład z tego przystanku
                  </Link>

                  <div className="d-flex flex-wrap mt-2" style={{ gap: "0.3rem" }}>
                    {routes.map((route) => {
                      const bgColor = feedColors[route.feedId] || "#ccc";
                      const textColor =
                        bgColor.toLowerCase() === "#ffffff" ? "#000" : "#fff";
                      return (
                        <Link
                          key={route.routeId}
                          to={`/route/${route.feedId}/${route.routeId}`}
                          className="px-2 py-1 rounded-pill text-decoration-none"
                          style={{
                            backgroundColor: bgColor,
                            color: textColor,
                            border: "1px solid #aaa",
                            fontSize: "0.75rem",
                            minWidth: "30px",
                            textAlign: "center",
                          }}
                        >
                          {route.routeShortName}
                        </Link>
                      );
                    })}
                  </div>
                </div>
              </Popup>
            )}
        </Marker>
      ))}

      {vehicles.map((vehicle) => (
        <Marker
          key={vehicle.fleetNumber}
          position={[vehicle.latitude, vehicle.longitude]}
          icon={busIcon}
        >
          <Popup>
            <div>
              <strong>{vehicle.fleetNumber}</strong>
              <br />
              Linia: {vehicle.routeId}
              <br />
              Kierunek: {vehicle.directionName}
            </div>
          </Popup>
        </Marker>
      ))}
    </MapContainer>
  );
}

export default StopsMap;
