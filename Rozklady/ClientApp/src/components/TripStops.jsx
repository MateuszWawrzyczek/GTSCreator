import React, { useEffect, useState } from "react";
import { useParams, Link } from "react-router-dom";
import StopsMap from "./StopsMap.jsx";
import "../styles/StopsList.css"; // opcjonalnie, jeśli masz własne style
//import { map } from "leaflet";

function TripStops() {
  const { feedId, routeId } = useParams();
  //const [variants, setVariants] = useState([]);
  const [timetable, setTimetable] = useState(null);
  const [loading, setLoading] = useState(true);
  //const [selectedVariantIndex, setSelectedVariantIndex] = useState(null);
  const apiUrl = process.env.REACT_APP_API_URL;

  useEffect(() => {
    const fetchVariants = async () => {
      try {
        const url = `${apiUrl}/api/Trip/${feedId}/${routeId}`;
        const response = await fetch(url);
        const data = await response.json();
        console.log("Pobrane dane:", url, data);
        // const parsed = data.map(v => ({
        //   ...v,
        //   stops: JSON.parse(v.stops)
        // }));
        setTimetable(data);
        //setVariants(parsed);
        //if (parsed.length > 0) setSelectedVariantIndex(0);
      } catch (err) {
        console.error(err);
      } finally {
        setLoading(false);
      }
    };

    fetchVariants();
  }, [feedId, routeId, apiUrl]);

  function formatTime(timeStr) {
    if (!timeStr) return "";
    const [h, m] = timeStr.split(":"); 
    return `${h.padStart(2, "0")}:${m.padStart(2, "0")}`;
    }

  if (loading) return <p>Ładowanie rozkładu...</p>;
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
        <h2 className="mb-3">  
          Linia {timetable && timetable.length > 0 ? timetable[0].routeShortName : ""}
        </h2>


        <div
          className="flex-grow-1 overflow-auto d-flex flex-column gap-3 mt-2"
          style={{ maxHeight: "calc(100vh - 56px - 3rem - 2rem)" }}
        >
          <ol className="stop-list">
            {timetable.map((stop, idx) => (
              <li key={stop.stop_id}>
                <div className="stop-content">
                {formatTime(stop.departureTime)}{" "}
                  <Link
                    to={`/stop/${feedId}/${stop.stopId}`}
                    className="stop-link"
                  >
                     {stop.stopName}
                  </Link>
                </div>
              </li>
            ))}
          </ol>
        </div>
      </div>

      <div className="col-12 col-lg-9 p-0 d-flex flex-column">
        <div className="flex-grow-1">
          <StopsMap stops={timetable} />
        </div>
      </div>
    </div>
  );
}

export default TripStops;
