import React from "react";
import { Link } from "react-router-dom";

const Navbar = () => {
  return (
    <nav className="bg-blue-600 text-white shadow-md px-6 py-4 flex justify-between items-center">
      <div className="text-xl font-bold">
        🚍 Aplikacja Rozkłady
      </div>
      <div className="flex gap-4">
        <Link to="/" className="hover:underline">
          Rozkład linii
        </Link>
        {/* tu możesz dodać kolejne zakładki */}
      </div>
    </nav>
  );
};

export default Navbar;
