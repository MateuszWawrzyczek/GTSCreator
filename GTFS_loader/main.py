from loaders.agency_loader import load_agency
from loaders.calendar_loader import load_calendar
from loaders.calendar_dates_loader import load_calendar_dates
from loaders.stops_loader import load_stops
from loaders.routes_loader import load_routes
from loaders.trips_loader import load_trips
from loaders.stop_times_loader import load_stop_times
from loaders.feed_info_loader import load_feed_info

from db import get_connection
from tqdm import tqdm  # ✅ pasek postępu
import os
import zipfile
import io
import csv


def delete_feed(feed_id: str):
    conn = get_connection()
    cur = conn.cursor()

    tables = [
        "stop_times",
        "trips",
        "routes",
        "stops",
        "calendar_dates",
        "calendar",
        "agency",
        "feed_info"
    ]

    print(f"🧹 Usuwam stare dane dla feed_id={feed_id}...")
    for table in tqdm(tables, desc="Usuwanie danych", ncols=80, colour="red"):
        cur.execute(f"DELETE FROM {table} WHERE feed_id = %s;", (feed_id,))

    conn.commit()
    cur.close()
    conn.close()
    print(f"✅ Usunięto stare dane dla feed_id={feed_id}")


def process_csv_with_progress(feed_id, file, loader_func, desc):
    """Czyta plik CSV i pokazuje pasek postępu."""
    text = io.TextIOWrapper(file, encoding="utf-8-sig")
    reader = list(csv.reader(text))
    total = len(reader) - 1  # pomijamy nagłówek

    # resetujemy wskaźnik pliku, by loader mógł odczytać całość
    file.seek(0)

    with tqdm(total=total, desc=desc, unit="wiersze", ncols=80, colour="green") as pbar:
        # możemy „opakować” loadera w callback aktualizujący pasek postępu
        def progress_callback():
            pbar.update(1)

        # loader otrzyma feed_id, plik i funkcję aktualizacji
        loader_func(feed_id, file, progress_callback)


def load_all(feed_id, zip_path):
    loaders = {
        "feed_info.txt": load_feed_info,
        "agency.txt": load_agency,
        "stops.txt": load_stops,
        "routes.txt": load_routes,
        "calendar.txt": load_calendar,
        "calendar_dates.txt": load_calendar_dates,
        "trips.txt": load_trips,
        "stop_times.txt": load_stop_times
    }

    with zipfile.ZipFile(zip_path, "r") as z:
        print("📦 Wczytywanie danych z pliku GTFS...")

        for filename, loader_func in loaders.items():
            if filename not in z.namelist():
                continue

            with z.open(filename) as f:
                # jeśli loader obsługuje callback – pokaż pasek postępu
                try:
                    process_csv_with_progress(feed_id, f, loader_func, filename)
                except TypeError:
                    # fallback dla loaderów bez callbacka
                    f.seek(0)
                    loader_func(feed_id, f)

    print("✅ Wczytano dane z pliku GTFS.")


def main():
    base_dir = os.path.dirname(os.path.abspath(__file__))
    folder = os.path.join(base_dir, "..", "GTFS-creator", "GotoweGTFS-y")
    print("Szukam folderu:", folder)

    gtfs_files = [f for f in os.listdir(folder) if f.endswith(".zip")]

    if not gtfs_files:
        print("❌ Brak plików GTFS w folderze:", folder)
        return

    print("Dostępne pliki GTFS:")
    for i, file in enumerate(gtfs_files, start=1):
        print(f"{i}. {file}")

    choice = input("Wybierz numer pliku: ")
    try:
        idx = int(choice) - 1
        if idx < 0 or idx >= len(gtfs_files):
            print("❌ Niepoprawny numer.")
            return
    except ValueError:
        print("❌ Musisz podać liczbę.")
        return

    zip_path = os.path.join(folder, gtfs_files[idx])
    print(f"📁 Wybrano: {zip_path}")
    feed_id = input("Podaj feed id: ")

    delete_feed(feed_id)
    load_all(feed_id, zip_path)


if __name__ == "__main__":
    main()
