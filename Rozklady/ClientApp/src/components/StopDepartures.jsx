import React, { useState, useEffect } from "react";
import { useParams } from "react-router-dom";

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
  const [lines, setLines] = useState([]); 
  const [departures, setDepartures] = useState([]);
  const [expanded, setExpanded] = useState(false);
  const [selectedLineIndex, setSelectedLineIndex] = useState(0);
  const [view, setView] = useState("timetable"); 

  const fetchDepartures = async () => {
    const today = new Date().toISOString().split("T")[0];
    const url = `https://localhost:7002/api/timetable/departures?date=${today}&feedId=${feedId}&stopId=${stopId}&hours=10&max=20`;
    const res = await fetch(url);
    if (!res.ok) {
      console.error("Błąd w API:", res.status, res.statusText);
      return;
    }
    const data = await res.json();
    console.log("Fetched departures:", data);
    setStopName(data.stopName)
    setDepartures(data.departures);
  };

  const fetchTimetable = async () => {
    console.log("Fetching timetable for", feedId, stopId);
    const url = `https://localhost:7002/api/StopTimetable/stop/${feedId}/${stopId}/timetable`;
    const res = await fetch(url);
        if (!res.ok) {
          throw new Error(`API error ${res.status}`);
        }
    const data = await res.json();
    console.log("Fetched timetable:", url);
    setLines(Array.isArray(data.lines) ? data.lines : []);
    setSelectedLineIndex(0);
  };

  const toggle = async () => {
    if (!expanded) {
      const tasks = [];
      if (!lines.length) tasks.push(fetchTimetable());
      if (!departures.length) tasks.push(fetchDepartures());
      await Promise.all(tasks);
    }
    setExpanded(prev => !prev);
  };

  useEffect(() => {
    fetchTimetable();   
    fetchDepartures();  
  }, [feedId, stopId]); 


  const toggleView = () => {
    setView((prev) => (prev === "departures" ? "timetable" : "departures"));
  };

  return (
    <div >
      <h2 className="mb-2">{stopName}</h2>
      <button onClick={toggleView} className="btn btn-sm btn-outline-primary">
        {view === "departures" ? "Rozkład jazdy" : "Tablica odjazdów"}
      </button>
      {view === "departures" && (
        <table className="table table-sm mt-3">
          <thead>
            <tr>
              <th>Linia</th>
              <th>Kierunek</th>
              <th>Godzina</th>
            </tr>
          </thead>
          <tbody>
            {departures.map((d, i) => (
              <tr key={i}>
                <td>{d.routeShortName}</td>
                <td>{d.headsign}</td>
                <td>{d.departureTime.substring(0, 5)}</td>
              </tr>
            ))}
          </tbody>
        </table>
      )}
      {view === "timetable" && lines[selectedLineIndex] && (
        <div className="mt-3">
          <div className="d-flex flex-wrap mb-3" style={{ gap: "0.3rem", rowGap: "0.3rem" }}>
            {lines.map((line, index) => {
              {console.log("Rendering line:", line)}
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
                      //onClick={() => handleTimeClick(dep)}
                      title={`${dep.tripId} — ${dep.feedId}`}
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