import React, { useEffect, useState } from "react";
import feedColors from "../styles/feedColors.js";
import "../styles/Brigades.css";
const dayTypeLabels = {
  RB: "Dni robocze w dni nauki szkolnej",
  RF: "Dni robocze wolne od nauki szkolnej",
  SB: "Soboty",
  ND: "Niedziele",
  SW: "Święta",
};


function Brigades() {
  const [blocks, setBlocks] = useState([]);
  const [loading, setLoading] = useState(true);
  const [activeDayType, setActiveDayType] = useState("RB"); 
  const [selectedBlockId, setSelectedBlockId] = useState(null);

  const apiUrl = process.env.REACT_APP_API_URL;

  useEffect(() => {
    const fetchBlocks = async () => {
      try {
        setLoading(true);
        const response = await fetch(`${apiUrl}/api/blocks`);
        const data = await response.json();
        setBlocks(data);
      } catch (err) {
        console.error(err);
      } finally {
        setLoading(false);
      }
    };
    fetchBlocks();
  }, [apiUrl]);

  if (loading) return <p>Ładowanie brygad...</p>;
  if (!blocks.length) return <p>Brak danych do wyświetlenia</p>;

  const groupedByDayType = blocks.reduce((acc, b) => {
    const dayType = b.blockId.slice(-2).toUpperCase(); 
    if (!acc[dayType]) acc[dayType] = [];
    acc[dayType].push(b);
    return acc;
  }, {});

  const blocksForDay = groupedByDayType[activeDayType] || [];

  const renderBlockList = () => {
    const kmrBlocks = blocksForDay.filter(b => b.trips[0].feedId === "KMR");
    const mzkBlocks = blocksForDay.filter(b => b.trips[0].feedId === "MZK");

    const renderColumn = (blocksArray, feedName) => (
      <div className="feed-column">
        <h5 className="mb-2">{feedName}</h5>
        <div
          style={{
            display: "flex",
            flexWrap: "wrap",
            justifyContent: "space-between",
            gap: "0.3rem",
            rowGap: "0.3rem",
          }}
        >
          {blocksArray.map((b) => {
            const representativeTrip = b.trips[0];
            const bgColor = feedColors[representativeTrip.feedId] || "#ccc";
            const textColor = bgColor.toLowerCase() === "#ffffff" ? "#000" : "#fff";

            return (
              <div
                key={b.blockId}
                className="px-2 py-1 rounded-pill fw-bold cursor-pointer"
                style={{
                  backgroundColor: bgColor,
                  color: textColor,
                  border: "1px solid #aaa",
                  textAlign: "center",
                  fontSize: "0.8rem",
                  lineHeight: "1rem",
                  whiteSpace: "nowrap",
                }}
                onClick={() => setSelectedBlockId(b.blockId)}
              >
                {b.blockId}
              </div>
            );
          })}
        </div>
      </div>
    );



    return (
      <div className="d-flex feed-columns">
        {renderColumn(kmrBlocks, "KMR")}
        {renderColumn(mzkBlocks, "MZK")}
      </div>
    );
  };



  const renderBlockDetails = () => {
    const block = blocksForDay.find(b => b.blockId === selectedBlockId);
    if (!block) return <p>Brak szczegółów dla tej brygady</p>;

    return (
      <div>
        <button
          className="mb-3 px-2 py-1 rounded border bg-light"
          onClick={() => setSelectedBlockId(null)}
        >
          ← Powrót do listy brygad
        </button>
        <h4 className="mb-2">Brygada {block.blockId}</h4>
        {block.trips
          .sort((a, b) => (a.startTime || "").localeCompare(b.startTime || ""))
          .map(trip => (
            <div
              key={trip.tripId || trip.blockId + trip.routeId + trip.startTime}
              className="mb-2 p-2 border rounded"
            >
              <div>
                <strong>Linia:</strong> {trip.routeId} — {trip.tripHeadsign}
              </div>
              <div>
                <strong>Czas:</strong> {trip.startTime} → {trip.endTime}
              </div>
            </div>
          ))}
      </div>
    );
  };

  return (
    <div className="p-3">
      {!selectedBlockId && (
        <div className="d-flex flex-wrap gap-2 mb-4 day-buttons">
          {Object.entries(dayTypeLabels).map(([dt, label]) => (
            <button
              key={dt}
              onClick={() => setActiveDayType(dt)}
              className="mb-3 px-2 py-1 rounded border"
              style={{
                backgroundColor: activeDayType === dt ? "#cce5ff" : "#f8f9fa", // delikatne podświetlenie
                color: activeDayType === dt ? "#004085" : "#212529",
                borderColor: "#aaa",
                fontWeight: activeDayType === dt ? "600" : "400",
              }}
            >
              {label}
            </button>
          ))}
        </div>
      )}
      {selectedBlockId ? renderBlockDetails() : renderBlockList()}
    </div>
  );
}


export default Brigades;
