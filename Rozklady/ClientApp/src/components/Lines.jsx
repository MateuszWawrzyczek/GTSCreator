  import React, { useEffect, useState } from "react";
  import { Link } from "react-router-dom";

  const feedColors = {
    MZK: "#DA251C",
    BKM: "#FFFFFF",
    PPszczyna: "#97D700",
    KMR: "#154BA1",
    PKR: "#AFCA0B",
    UMPszczyna: "#3683CF",
    PKSR: "#F8C301",
    PWodzisławski: "#17D9E5",
    DRAB: "#000000",
    DRAB_E: "#000000",
    BROTHERS: "#FD7C18",
    WIS: "#022b50ff",
    SWIERKLANY: "#474B8A",
    LINEA: "#BD0224",
  };

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
      <div className="p-3">
        <h2>Rozkład linii</h2>
        <div
          className="d-flex flex-wrap mt-2"
          style={{ justifyContent: "space-between", gap: "0.3rem", rowGap: "0.3rem" }}
        >
          {linie.map((linia) => {
            const bgColor = feedColors[linia.feedId] || "#ccc";
            const textColor = bgColor.toLowerCase() === "#ffffff" ? "#000" : "#fff";

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
    );
  }

  export default Lines;
