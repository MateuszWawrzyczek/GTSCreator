import React, { useEffect, useState } from "react";
import RouteListItem from "./RouteListItem";

const feedColors = {
    MZK: "#DA251C",
    BKM: "#FFFFFF",
    PowiatPszczyna: "#97D700",
    KMRybnik: "#154BA1",
    PKRACIBORZ: "#AFCA0B",
    UMPSZCZYNA: "#3683CF",
    PKSRACIBORZ: "#F8C301",
    POWIATWODZISLAWSKI: "#17D9E5",
    DRAB: "#000000",
    DRABE: "#000000",
    BROTHERS: "#FD7C18",
    WISPOL: "#022b50ff",
    KGSWIERKLANY: "#474B8A",
    LINETRANS: "BD0224",
};

function Sidebar() {
  const [routes, setRoutes] = useState([]);

  useEffect(() => {
    fetch("/api/routes")
      .then((res) => res.json())
      .then((data) => setRoutes(data));
  }, []);

  return (
    <div style={styles.sidebar}>
      {routes.map((r) => (
        <RouteListItem
          key={r.routeId}
          route={r}
          color={feedColors[r.feedId] || "#ccc"}
        />
      ))}
    </div>
  );
}

const styles = {
  sidebar: {
    width: "200px",
    padding: "1rem",
    borderRight: "1px solid #ccc",
    display: "flex",
    flexDirection: "column",
    gap: "10px",
  },
};

export default Sidebar;
