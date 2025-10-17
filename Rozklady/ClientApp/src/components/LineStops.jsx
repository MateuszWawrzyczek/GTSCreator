import React, { useEffect, useState } from "react";
import { useParams, Link } from "react-router-dom";
import StopsMap from "./StopsMap.jsx";
import "../styles/StopsList.css"; 

function RouteVariantStops() {
  const { feedId, routeId } = useParams();
  const [variants, setVariants] = useState([]);
  const [vehicles, setVehicles] = useState([]);
  
  const [loading, setLoading] = useState(true);
  const [selectedVariantIndex, setSelectedVariantIndex] = useState(null);
  const apiUrl = process.env.REACT_APP_API_URL;

  useEffect(() => {
    const fetchVariants = async () => {
      try {
        const url = `${apiUrl}/api/RouteStops?feedId=${feedId}&routeId=${routeId}`;
        const response = await fetch(url);
        const data = await response.json();

        const parsed = data.map(v => ({
          ...v,
          stops: JSON.parse(v.stops)
        }));

        setVariants(parsed);
        if (parsed.length > 0) setSelectedVariantIndex(0);
        console.log(parsed);
      } catch (err) {
        console.error(err);
      } finally {
        setLoading(false);
      }
    };

    const fetchVehicles = async () => {
      const res = await fetch(`${apiUrl}/api/vehicles/vehiclePositions`);
      const data = await res.json();
      setVehicles(data);
    };

    fetchVariants();
    fetchVehicles();
  }, [feedId, routeId, apiUrl]);

  const filteredVehicles = React.useMemo(() => {
    return vehicles.filter(v => v.routeId === routeId);
  }, [vehicles, routeId]);

  if (loading) return <p>Ładowanie wariantów...</p>;
  if (!variants.length) return <p>Brak wariantów dla tej linii</p>;

  const handleChange = (e) => {
    const index = e.target.value === "" ? null : Number(e.target.value);
    setSelectedVariantIndex(index);
  };

  const selectedVariant = selectedVariantIndex !== null ? variants[selectedVariantIndex] : null;
  console.log(selectedVariant);
    const mappedStops = selectedVariant?.stops
    .filter(s => s.lat != null && s.lon != null)
    .map(s => ({
      feedId: feedId,
      stopId: s.stop_id,
      stopName: s.stop_name,
      stopLat: s.lat,
      stopLon: s.lon,
    }));

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
        <h2 className="mb-3">Warianty linii {variants[0]?.routeShortName}</h2>

        <select
          className="form-select mb-3"
          onChange={handleChange}
          value={selectedVariantIndex !== null ? selectedVariantIndex : ""}
        >
          <option value="">-- Wybierz wariant --</option>
          {variants.map((v, i) => {
            const firstStop = v.stops[0]?.stop_name || "-";
            const lastStop = v.stops[v.stops.length - 1]?.stop_name || "-";
            return (
              <option key={i} value={i}>
                {firstStop} → {lastStop}
              </option>
            );
          })}
        </select>

        <div
          className="flex-grow-1 overflow-auto d-flex flex-column gap-3 mt-2"
          style={{ maxHeight: "calc(100vh - 56px - 3rem - 2rem)" }}
        >
          <ol className="stop-list">
            {selectedVariant?.stops.map((stop, idx) => (
              <li key={stop.stop_id}>
                <div className="stop-content">
                  <Link
                    to={`/stop/${feedId}/${stop.stop_id}`}
                    className="stop-link"
                  >
                    {idx + 1}. {stop.stop_name}
                  </Link>
                </div>
              </li>
            ))}
          </ol>
        </div>
      </div>

      <div className="col-12 col-lg-9 p-0 d-flex flex-column">
        <div className="flex-grow-1">
          <StopsMap stops={mappedStops} vehicles={filteredVehicles}/>
        </div>
      </div>
    </div>
  );
}

export default RouteVariantStops;
