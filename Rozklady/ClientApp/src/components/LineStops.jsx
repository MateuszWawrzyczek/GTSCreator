import React, { useEffect, useState } from "react";
import { useParams } from "react-router-dom";

function RouteVariantsDropdown() {
  const { feedId, routeId } = useParams();
  const [variants, setVariants] = useState([]);
  const [loading, setLoading] = useState(true);
  const [selectedVariantIndex, setSelectedVariantIndex] = useState(null);

  useEffect(() => {
    const fetchVariants = async () => {
      try {
        const url = `https://localhost:7002/api/RouteStops?feedId=${feedId}&routeId=${routeId}`;
        const response = await fetch(url);
        const data = await response.json();

        const parsed = data.map(v => ({
          ...v,
          stops: JSON.parse(v.stops)
        }));

        setVariants(parsed);
      } catch (err) {
        console.error(err);
      } finally {
        setLoading(false);
      }
    };

    fetchVariants();
  }, [feedId, routeId]);

  if (loading) return <p>Ładowanie wariantów...</p>;
  if (!variants.length) return <p>Brak wariantów dla tej linii</p>;

  const handleChange = (e) => {
    const index = e.target.value === "" ? null : Number(e.target.value);
    setSelectedVariantIndex(index);
  };

  const selectedVariant = selectedVariantIndex !== null ? variants[selectedVariantIndex] : null;

  return (
    <div className="p-3">
      <h2>Warianty linii {routeId}</h2>

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

      {selectedVariant && (
        <div>
          <h3>Przystanki:</h3>
          <ol>
            {selectedVariant.stops.map((stop) => (
              <li key={stop.stop_id}>{stop.stop_name}</li>
            ))}
          </ol>
        </div>
      )}
    </div>
  );
}

export default RouteVariantsDropdown;
