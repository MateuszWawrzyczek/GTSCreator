import React, { useState } from "react";
import 'bootstrap/dist/css/bootstrap.min.css';
import { Link } from "react-router-dom";

function Navbar() {
  const [isOpen, setIsOpen] = useState(false);

  return (
    <header className="w-100">

      <div className="bg-success text-white small d-flex justify-content-between px-4 py-1">
        <span>📧 kontakt@mytabor.pl</span>
      </div>

      <div className="d-flex align-items-center px-4 py-3">
        <img src="/logo192.png" alt="Logo" className="me-3" style={{ height: '48px' }} />
        <h1 className="h5 fw-bold text-success mb-0">
          ROZKŁAD JAZDY - SUBREGION ZACHODNI
        </h1>

        <button
          className="btn btn-success ms-auto d-lg-none"
          onClick={() => setIsOpen(!isOpen)}
        >
          ☰
        </button>
      </div>

      <nav className={`bg-success p-2 ${isOpen ? "" : "d-none d-lg-block"}`}>
        <ul className="nav flex-column flex-lg-row">
          <li className="nav-item">
            <Link 
              to="/linie" 
              className="nav-link text-white fw-semibold"
              onClick={() => setIsOpen(false)}
            >
              Rozkład linii
            </Link>
          </li>
          <li className="nav-item">
            <Link 
              to="/przystanki" 
              className="nav-link text-white fw-semibold"
              onClick={() => setIsOpen(false)}
            >
              Przystanki
            </Link>
          </li>
          <li className="nav-item">
            <Link 
              to="/lista-pojazdow" 
              className="nav-link text-white fw-semibold"
              onClick={() => setIsOpen(false)}
            >
              Pojazdy na żywo
            </Link>
          </li>
          <li className="nav-item">
            <Link 
              to="/brygady" 
              className="nav-link text-white fw-semibold"
              onClick={() => setIsOpen(false)}
            >
              Brygady
            </Link>
          </li>
        </ul>
      </nav>

    </header>
  );
}

export default Navbar;
