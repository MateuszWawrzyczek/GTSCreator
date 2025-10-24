import React, { useState, useEffect } from "react";
import { useParams, useNavigate } from "react-router-dom";

import feedColors from "../styles/feedColors.js";

const DAY_TYPE_LABELS = {
  RS: "Dni robocze w dni nauki szkolnej",
  RW: "Dni robocze wolne od nauki szkolnej",
  S:  "Soboty",
  N:  "Niedziele",
  Sw: "Święta"
};

function StopDepartures() {
  const { feedId, stopId } = useParams();
  const [stopName, setStopName] = useState("");
  const [loading, setLoading] = useState(true);
  const [lines, setLines] = useState([]); 
  const [departures, setDepartures] = useState([]);
  const [selectedLineIndex, setSelectedLineIndex] = useState(0);
  const [view, setView] = useState("timetable"); 
  const apiUrl = process.env.REACT_APP_API_URL;


  useEffect(() => {
    const fetchTimetable = async () => {
      try {
        const url = `${apiUrl}/api/StopTimetable/stop/${feedId}/${stopId}/timetable`;
        const res = await fetch(url);
        if (!res.ok) throw new Error(`API error ${res.status}`);
        const data = await res.json();
        setLines(Array.isArray(data.lines) ? data.lines : []);
        setSelectedLineIndex(0);
      } catch (err) {
        console.error("Błąd pobierania rozkładu:", err);
      }
    };

    fetchTimetable();
  }, [feedId, stopId, apiUrl]);

  useEffect(() => {
    const fetchDepartures = async () => {
      try {
        const today = new Date().toISOString().split("T")[0];
        const url = `${apiUrl}/api/timetable/departures?date=${today}&feedId=${feedId}&stopId=${stopId}&hours=24&max=20`;
        const res = await fetch(url);
        if (!res.ok) throw new Error(`API error ${res.status}`);
        const data = await res.json();
        setStopName(data.stopName || "");
        setDepartures(Array.isArray(data.departures) ? data.departures : []);
      } catch (err) {
        console.error("Błąd pobierania odjazdów:", err);
      }
    };

    fetchDepartures();
    const interval = setInterval(fetchDepartures, 5000);
    return () => clearInterval(interval);
  }, [feedId, stopId, apiUrl]);

  const navigate = useNavigate();

  const toggleView = () => {
    setView((prev) => (prev === "departures" ? "timetable" : "departures"));
  };

  const formatDepartureTime = (departureTime, delay, onTrip) => {
    if (onTrip && delay != null) {
      const [dh, dm, ds] = delay.replace("-", "").split(":").map(Number);
      const totalDelaySeconds = dh * 3600 + dm * 60 + ds;
      const delaySign = delay.startsWith("-") ? -1 : 1;

      const now = new Date();
      const dep = new Date();
      const [h, m, s] = departureTime.split(":").map(Number);
      dep.setHours(h, m, s || 0, 0);

      const diffMinutes = ((dep - now) / 60000) - (delaySign * totalDelaySeconds / 60);
      const diffRounded = Math.max(Math.round(diffMinutes), 0);

      if (diffMinutes > 0 && diffMinutes < 1) {
        return "<1 min";
      }

      return `${diffRounded} min`;
    }

    return departureTime.substring(0, 5);
  };


  return (
    <div >
      <h2 className="mb-2">{stopName}</h2>
      <button onClick={toggleView} className="btn btn-sm btn-outline-primary">
        {view === "departures" ? "Rozkład jazdy" : "Tablica odjazdów"}
      </button>
      {view === "departures" && (
        <div className="mt-3">
          {departures.length > 0 ? (
            <table className="table table-sm">
              <thead>
                <tr>
                  <th>Linia</th>
                  <th>Numer tab.</th>
                  <th>Kierunek</th>
                  <th>Godzina</th>
                </tr>
              </thead>
              <tbody>
                {departures.map((d, i) => (
                  <tr key={i}>
                    <td>{d.routeShortName}</td>
                    <td>{d.fleetNumber || "-"}</td>
                    <td>{d.headsign}</td>
                    <td>{formatDepartureTime(d.departureTime, d.delay, d.onTrip)}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          ) : (
            <div
              className="text-center text-muted py-4"
              style={{
                fontStyle: "italic",
                fontSize: "0.95rem",
                backgroundColor: "#f9f9f9",
                borderRadius: "10px",
                border: "1px solid #eee",
              }}
            >
              🕓 Dzisiaj z tego przystanku nie odjedzie już żaden autobus
            </div>
          )}
        </div>
      )}

      {view === "timetable" && lines[selectedLineIndex] && (
        <div className="mt-3">
          <div className="d-flex flex-wrap mb-3" style={{ gap: "0.3rem", rowGap: "0.3rem" }}>
            {lines.map((line, index) => {
              const bgColor = feedColors[line.feedId] || "#ccc";
              const textColor = bgColor.toLowerCase() === "#ffffff" ? "#000" : "#fff";
              const isSelected = index === selectedLineIndex;

              return (
                <button
                  key={line.route}
                  onClick={() => setSelectedLineIndex(index)}
                  className={`px-3 py-1 rounded-pill fw-bold border ${
                    isSelected ? "border-dark" : "border-light"
                  }`}
                  style={{
                    backgroundColor: bgColor,
                    color: textColor,
                    minWidth: "40px",
                    textAlign: "center",
                    fontSize: "0.8rem",
                    lineHeight: "1rem",
                  }}
                >
                  {line.route}
                </button>
              );
            })}
          </div>

          {["RS","RW","S","N","Sw"].map((dt) => {
            const list = lines[selectedLineIndex].days?.[dt];
            if (!list || !list.length) return null; 

            const sorted = [...list].sort((a, b) =>
              (a.time || "").substring(0,5).localeCompare((b.time || "").substring(0,5))
            );

            return (
              <div key={dt} className="mb-3">
                <div className="fw-semibold mb-2">{DAY_TYPE_LABELS[dt] || dt}</div>
                <div className="d-flex flex-wrap gap-2">
                  {sorted.map(dep => (
                    <button
                      key={`${dep.feedId}_${dep.tripId}_${dep.time}`}
                      className="btn btn-sm rounded-pill time-pill"
                      title={`${dep.tripId} — ${dep.feedId}`}
                      onClick={() => navigate(`/trip/${dep.feedId}/${dep.tripId}?selectedStop=${stopId}`)}
                    >
                      {(dep.time || "").substring(0,5)}
                    </button>
                  ))}
                </div>
              </div>
            );
          })}
        </div>
      )}
    </div>
  );
}

export default StopDepartures;