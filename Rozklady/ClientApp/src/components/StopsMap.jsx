import { useEffect, useState } from "react";
import { MapContainer, TileLayer, Marker, Popup } from "react-leaflet";
import L from "leaflet";
import { Link } from "react-router-dom";
import "leaflet/dist/leaflet.css";
import feedColors from "../styles/feedColors.js";

const busStopIcon = new L.Icon({
  iconUrl: "/icons/bus-stop.svg",
  iconSize: [24, 24],
  iconAnchor: [16, 32],
  popupAnchor: [0, -32],
});

function StopsMap({ stops }) {
  const [routesByStop, setRoutesByStop] = useState({});
  const defaultCenter = [50.025, 18.54];

  useEffect(() => {
    const fetchRoutes = async () => {
      for (const stop of stops) {
        const key = `${stop.feedId}_${stop.stopId}`;
        if (!routesByStop[key]) {
          try {
            const url = `https://localhost:7002/api/stops/${stop.feedId}/${stop.stopId}/routes`;
            const res = await fetch(url, { mode: "cors" });
            if (!res.ok) throw new Error("Błąd pobierania linii");
            const data = await res.json();
            setRoutesByStop((prev) => ({ ...prev, [key]: data }));
          } catch (err) {
            console.error(err);
          }
        }
      }
    };

    if (stops.length) {
      fetchRoutes();
    }
  }, [stops]);

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

      {stops?.map((stop) => {
        const key = `${stop.feedId}_${stop.stopId}`;
        const routes = routesByStop[key] || [];

        return (
          <Marker
            key={key}
            position={[stop.stopLat, stop.stopLon]}
            icon={busStopIcon}
          >
            <Popup>
              <div style={{ minWidth: "180px" }}>
                <strong>{stop.stopName}</strong>
                <br />
                <Link
                  to={`/stop/${stop.feedId}/${stop.stopId}`}
                  className="text-decoration-none"
                >
                  Rozkład z tego przystanku
                </Link>
                <div
                  className="d-flex flex-wrap mt-2"
                  style={{ gap: "0.3rem", rowGap: "0.3rem" }}
                >
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
          </Marker>
        );
      })}
    </MapContainer>
  );
}

export default StopsMap;
