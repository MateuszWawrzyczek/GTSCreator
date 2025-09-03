import React from "react";
import Navbar from "./Navbar";

const AppLayout = ({ children }) => {
  return (
    <div className="flex flex-col h-screen">
      {/* Górne menu */}
      <Navbar />

      {/* Główna zawartość */}
      <div className="flex flex-1">
        {/* Lewy panel (np. linie) */}
        <aside className="w-1/3 border-r border-gray-300 p-4 overflow-y-auto">
          <h2 className="text-lg font-semibold mb-4">Lista linii</h2>
          {/* Tu będą elementy (np. owalne boxy z numerami linii) */}
        </aside>

        {/* Prawa część (np. mapa) */}
        <main className="flex-1 p-4">
          {children}
        </main>
      </div>
    </div>
  );
};

export default AppLayout;
