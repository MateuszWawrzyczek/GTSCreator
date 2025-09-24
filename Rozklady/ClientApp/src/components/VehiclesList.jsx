import React, { useState, useEffect } from "react";

function VehiclesList() {
  const [vehicles, setVehicles] = useState([]);
  const [loading, setLoading] = useState(true);
  const [sortConfig, setSortConfig] = useState({ key: null, direction: "asc" });

  useEffect(() => {
    const fetchVehicles = async () => {
      try {
        const res = await fetch("https://localhost:7002/api/vehicles/vehiclePositions");
        if (!res.ok) throw new Error(`API error ${res.status}`);
        const data = await res.json();
        const normalized = data.map(v => ({
          ...v,
          directionName: v.directionName?.replace(/&#211;/g, "Ó")
        }));

        setVehicles(normalized);
        setLoading(false);
      } catch (err) {
        console.error("Błąd pobierania pojazdów:", err);
      }
    };

    fetchVehicles();
    const interval = setInterval(fetchVehicles, 5000); 
    return () => clearInterval(interval);
  }, []);

  const sortedVehicles = React.useMemo(() => {
    if (!vehicles) return [];
    if (!sortConfig.key) return vehicles;

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
      <table className="table table-sm mt-3">
        <thead>
          <tr>
            <th onClick={() => requestSort("fleetNumber")}>Numer taborowy{getSortArrow("fleetNumber")}</th>
            <th onClick={() => requestSort("routeId")}>Linia{getSortArrow("routeId")}</th>
            <th onClick={() => requestSort("directionName")}>Kierunek{getSortArrow("directionName")}</th>
            <th onClick={() => requestSort("model")}>Model{getSortArrow("model")}</th>
            <th onClick={() => requestSort("delay")}>Opóźnienie{getSortArrow("delay")}</th>
          </tr>
        </thead>
        <tbody>
          {sortedVehicles.map((v, i) => (
            <tr key={i}>
              <td>{v.fleetNumber}</td>
              <td>{v.routeId}</td>
              <td>{v.directionName}</td>
              <td>{v.model}</td>
              <td>{v.delay}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}

export default VehiclesList;
