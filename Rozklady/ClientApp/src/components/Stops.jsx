import React, { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import "../styles/StopsList.css";
import feedColors from "../styles/feedColors.js"
import StopsMap from "./StopsMap.jsx";

function slugify(str) {
  return str
    .normalize("NFD")
    .replace(/[\u0300-\u036f]/g, "")
    .replace(/[^a-z0-9]/gi, "")
    .toLowerCase();
}

function Stops() {
  const [stops, setStops] = useState([]);
  const [loading, setLoading] = useState(true);
  const [search, setSearch] = useState("");
  const [expandedStops, setExpandedStops] = useState({}); 

  useEffect(() => {
    const fetchStops = async () => {
      try {
        const url = "https://localhost:7002/api/stops";
        const response = await fetch(url, { mode: "cors" });
        if (!response.ok) throw new Error("Błąd podczas pobierania danych");
        const data = await response.json();

        const withSlugs = data.map((s) => ({
          ...s,
          stopSlug: slugify(s.stopName),
        }));

        setStops(withSlugs);
      } catch (error) {
        console.error("Fetch error:", error);
      } finally {
        setLoading(false);
      }
    };

    fetchStops();
  }, []);

  const toggleStop = async (feedId, stopId) => {
    const key = `${feedId}_${stopId}`;
    if (expandedStops[key]) {
      setExpandedStops((prev) => {
        const copy = { ...prev };
        delete copy[key];
        return copy;
      });
    } else {
      try {
        const url = `https://localhost:7002/api/stops/${feedId}/${stopId}/routes`;
        const response = await fetch(url, { mode: "cors" });
        if (!response.ok) throw new Error("Błąd pobierania linii dla przystanku");
        const data = await response.json();
        setExpandedStops((prev) => ({ ...prev, [key]: data }));
      } catch (err) {
        console.error(err);
      }
    }
  };

  if (loading) return <p>Ładowanie przystanków...</p>;
  if (!stops.length) return <p>Brak danych do wyświetlenia</p>;

  const filteredStops = stops.filter((stop) =>
    stop.stopSlug.includes(slugify(search))
  );

  return (
    <div className="row" style={{ height: "calc(100vh - 56px)" }}> 
      <div
        className="col-12 col-lg-3 border-end d-flex flex-column"
        style={{
          paddingTop: "1rem",
          paddingBottom: "1rem",
          paddingLeft: "2.5rem",
          paddingRight: "1.5rem",
        }}
      >
        <h2 className="mb-3">Przystanki</h2>

        <input
          type="text"
          placeholder="Szukaj przystanku..."
          className="form-control mb-3"
          value={search}
          onChange={(e) => setSearch(e.target.value)}
        />

        <div
          className="flex-grow-1 overflow-auto d-flex flex-column gap-3 mt-2"
          style={{ maxHeight: "calc(100vh - 56px - 3rem - 2rem)" }}
        >
          {filteredStops.map((stop) => {
            const key = `${stop.feedId}_${stop.stopId}`;
            const expanded = expandedStops[key];

            return (
              <div key={key}>
                <div
                  onClick={() => toggleStop(stop.feedId, stop.stopId)}
                  style={{
                    cursor: "pointer",
                    display: "flex",
                    alignItems: "center",
                    justifyContent: "space-between",
                  }}
                >
                  <Link to={`/stop/${stop.feedId}/${stop.stopId}`} className="stop-link">
                    {stop.stopName}
                  </Link>
                  <span>{expanded ? "⌄" : "›"}</span>
                </div>

                {expanded && (
                  <div className="d-flex flex-wrap mt-1" style={{ gap: "0.3rem", rowGap: "0.3rem" }}>
                    {expanded.map((route) => {
                      const bgColor = feedColors[route.feedId] || "#ccc";
                      const textColor = bgColor.toLowerCase() === "#ffffff" ? "#000" : "#fff";

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
                )}
              </div>
            );
          })}
        </div>
      </div>

      <div className="col-12 col-lg-9 p-0 d-flex flex-column">
        <StopsMap stops={filteredStops} style={{ flexGrow: 1, height: "100%" }} />
      </div>
    </div>
  );

};

export default Stops;
