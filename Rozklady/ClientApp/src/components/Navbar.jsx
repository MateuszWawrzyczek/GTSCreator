import React from "react";
import 'bootstrap/dist/css/bootstrap.min.css';
import { Link } from "react-router-dom";

function Navbar() {
  return (
    <header className="w-100">

      {/* Pasek górny */}
      <div className="bg-success text-white small d-flex justify-content-between px-4 py-1">
        <span>📧 kontakt@mytabor.pl</span>
      </div>

      {/* Środkowy nagłówek */}
      <div className="d-flex align-items-center px-4 py-3">
        <img src="/logo192.png" alt="Logo" className="me-3" style={{ height: '48px' }} />
        <h1 className="h5 fw-bold text-success mb-0">
          ROZKŁAD JAZDY - SUBREGION ZACHODNI
        </h1>
      </div>

      {/* Pasek menu */}
      <nav className="bg-success p-2">
  <ul className="nav">
    <li className="nav-item">
      <Link to="/linie" className="nav-link text-white fw-semibold">
        Rozkład linii
      </Link>
    </li>
    <li className="nav-item">
      <Link to="/stops" className="nav-link text-white fw-semibold">
        Przystanki
      </Link>
    </li>
    <li className="nav-item">
      <Link to="/vehicles" className="nav-link text-white fw-semibold">
        Pojazdy na żywo
      </Link>
    </li>
  </ul>
</nav>

    </header>
  );
}

export default Navbar;
