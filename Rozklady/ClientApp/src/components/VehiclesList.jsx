import React, { useState, useEffect } from "react";

function parseDelayToSeconds(delayStr) {
  if (!delayStr || typeof delayStr !== "string") return 0;

  const match = delayStr.match(/(-?)(\d{2}):(\d{2}):(\d{2})/);
  if (!match) return 0;

  const sign = match[1] === "-" ? -1 : 1;
  const hours = parseInt(match[2], 10);
  const minutes = parseInt(match[3], 10);
  const seconds = parseInt(match[4], 10);

  return sign * (hours * 3600 + minutes * 60 + seconds);
}

function sortDelay(a, b, direction = "asc") {
  const aDelay = parseDelayToSeconds(a.delay);
  const bDelay = parseDelayToSeconds(b.delay);
  return direction === "asc" ? aDelay - bDelay : bDelay - aDelay;
}


function VehiclesList() {
  const [vehicles, setVehicles] = useState([]);
  const [loading, setLoading] = useState(true);
  const [sortConfig, setSortConfig] = useState({ key: "fleetNumber", direction: "asc" });
  const apiUrl = process.env.REACT_APP_API_URL;


  useEffect(() => {
    const fetchVehicles = async () => {
      try {
        const res = await fetch(`${apiUrl}/api/vehicles/vehiclePositions`);
        if (!res.ok) throw new Error(`API error ${res.status}`);
        const data = await res.json();
        const normalized = data.map(v => ({
          ...v,
          directionName: v.directionName?.replace(/&#211;/g, "Ó")
        }));
        console.log("Pobrano pojazdy:", normalized);
        setVehicles(normalized);
        setLoading(false);
      } catch (err) {
        console.error("Błąd pobierania pojazdów:", err);
      }
    };

    fetchVehicles();
    const interval = setInterval(fetchVehicles, 5000); 
    return () => clearInterval(interval);
  }, [apiUrl]);

const sortedVehicles = React.useMemo(() => {
  if (!vehicles) return [];
  if (!sortConfig.key) return vehicles;

  if (sortConfig.key === "delay") {
    return [...vehicles].sort((a, b) =>
      sortDelay(a, b, sortConfig.direction)
    );
  }

    return [...vehicles].sort((a, b) => {
      let aValue = a[sortConfig.key];
      let bValue = b[sortConfig.key];

      const aNum = parseFloat(aValue);
      const bNum = parseFloat(bValue);
      if (!isNaN(aNum) && !isNaN(bNum)) {
        return sortConfig.direction === "asc" ? aNum - bNum : bNum - aNum;
      }


      if (typeof aValue === "string") aValue = aValue.toLowerCase();
      if (typeof bValue === "string") bValue = bValue.toLowerCase();

      if (aValue < bValue) return sortConfig.direction === "asc" ? -1 : 1;
      if (aValue > bValue) return sortConfig.direction === "asc" ? 1 : -1;
      return 0;
    });
  }, [vehicles, sortConfig]);


  const requestSort = (key) => {
    let direction = "asc";
    if (sortConfig.key === key && sortConfig.direction === "asc") {
      direction = "desc";
    }
    setSortConfig({ key, direction });
  };

  const getSortArrow = (key) => {
    if (sortConfig.key !== key) return null;
    return sortConfig.direction === "asc" ? " ▲" : " ▼";
  };

  if (loading) return <p>Ładowanie pojazdów...</p>;
  if (!vehicles.length) return <p>Brak pojazdów w tym momencie</p>;

  return (
    <div className="container mt-2">
      <h2>Pojazdy na żywo</h2>
      <div className="table-responsive mt-3">
        <table className="table table-sm">
          <thead>
            <tr>
              <th onClick={() => requestSort("feedId")}>
                Organizator{getSortArrow("feedId")}
              </th>
              <th onClick={() => requestSort("fleetNumber")}>
                Nr. taborowy{getSortArrow("fleetNumber")}
              </th>
              <th onClick={() => requestSort("routeId")}>
                Linia{getSortArrow("routeId")}
              </th>
              <th onClick ={() => requestSort("blockId")}>
                Brygada{getSortArrow("blockId")}
              </th>
              <th onClick={() => requestSort("directionName")}>
                Kierunek{getSortArrow("directionName")}
              </th>
              <th onClick={() => requestSort("model")}>
                Model{getSortArrow("model")}
              </th>
              <th onClick={() => requestSort("delay")}>
                Opóźnienie{getSortArrow("delay")}
              </th>

            </tr>
          </thead>
          <tbody>
            {sortedVehicles.map((v, i) => {
              const delaySeconds = parseDelayToSeconds(v.delay);
              const getDelayStyle = () => {
                if (!v.onTrip) return {}; 
                if (delaySeconds >= 0) return { color: "green", fontWeight: "600" }; 
                if (delaySeconds < 0) return { color: "red", fontWeight: "600" }; 
                return {}; 
              };
              return (
                <tr key={i}>
                  <td>{v.feedId}</td>
                  <td>{v.fleetNumber}</td>
                  <td>{v.routeId}</td>
                  <td>{v.blockId || "-"}</td>
                  <td>{v.directionName}</td>
                  <td>{v.model}</td>
                  <td style={getDelayStyle()}>{v.delay}</td>
                </tr>
              );
            })}
          </tbody>
        </table>
      </div>
    </div>
  );

}

export default VehiclesList;
