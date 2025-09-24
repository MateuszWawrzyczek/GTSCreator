import React, { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import feedColors from "../styles/feedColors.js"
import StopsMap from "./StopsMap.jsx";

function Lines() {
  const [linie, setLinie] = useState([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const fetchLinie = async () => {
      try {
        const url = "https://localhost:7002/api/routes";
        console.log("Fetching data from:", url);
        const response = await fetch(url, { mode: "cors" });

        console.log("Fetching data from:", url);
        if (!response.ok) throw new Error("Błąd podczas pobierania danych");
        const data = await response.json();
        setLinie(data);
      } catch (error) {
        console.error("Fetch error:", error);
      } finally {
        setLoading(false);
      }
    };

    fetchLinie();
  }, []);

  if (loading) return <p>Ładowanie linii...</p>;
  if (!linie.length) return <p>Brak danych do wyświetlenia</p>;

  return (
  <div className="row" style={{ height: "calc(100vh - 56px)" }}>
    <div
      className="col-12 col-lg-3 border-end d-flex flex-column"
      style={{
          paddingTop: "1rem",
          paddingBottom: "1rem",
          paddingLeft: "2rem",
          paddingRight: "2rem",
        }}
    >
      <h1 className="mb-3">Linie</h1>
      <div
        className="d-flex flex-wrap mt-2"
        style={{ justifyContent: "space-between", gap: "0.3rem", rowGap: "0.3rem" }}
      >
        {linie.map((linia) => {
          const bgColor = feedColors[linia.feedId] || "#ccc";
          const textColor =
            bgColor.toLowerCase() === "#ffffff" ? "#000" : "#fff";

          return (
            <Link
              key={linia.routeId}
              to={`/route/${linia.feedId}/${linia.routeId}`}
              className="px-3 py-1 rounded-pill fw-bold text-decoration-none"
              style={{
                backgroundColor: bgColor,
                color: textColor,
                border: "1px solid #aaa",
                minWidth: "40px",
                textAlign: "center",
                fontSize: "0.8rem",
                lineHeight: "1rem",
              }}
            >
              {linia.routeShortName}
            </Link>
          );
        })}
      </div>
    </div>

    <div className="col-12 col-lg-9 p-0 d-flex flex-column">
      <StopsMap stops={[]} />
    </div>
  </div>
);
}

export default Lines;
