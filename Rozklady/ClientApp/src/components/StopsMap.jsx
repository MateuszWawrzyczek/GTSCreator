import { MapContainer, TileLayer, Marker, Popup } from "react-leaflet";
import "leaflet/dist/leaflet.css";

function StopsMap({ stops }) {
  const defaultCenter = [50.025, 18.54]; // środek Wodzisław/Żory
  const center = stops && stops.length > 0 ? defaultCenter : defaultCenter;

  return (
    <MapContainer center={center} zoom={12} style={{ height: "100%", width: "100%" }}>
      <TileLayer
        url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png"
        attribution='&copy; <a href="https://www.openstreetmap.org/">OpenStreetMap</a> contributors'
      />
      {stops?.map((stop) => (
        <Marker key={`${stop.feedId}_${stop.stopId}`} position={[stop.stopLat, stop.stopLon]}>
          <Popup>
            <strong>{stop.stopName}</strong><br />
            ID: {stop.stopId}
          </Popup>
        </Marker>
      ))}
    </MapContainer>
  );
}

export default StopsMap;
