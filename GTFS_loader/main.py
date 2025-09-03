from loaders.agency_loader import load_agency
from loaders.calendar_loader import load_calendar
from loaders.calendar_dates_loader import load_calendar_dates
from loaders.stops_loader import load_stops
from loaders.routes_loader import load_routes
from loaders.trips_loader import load_trips
from loaders.stop_times_loader import load_stop_times
from loaders.feed_info_loader import load_feed_info

from db import get_connection
import os

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

    for table in tables:
        cur.execute(f"DELETE FROM {table} WHERE feed_id = %s;", (feed_id,))

    conn.commit()
    cur.close()
    conn.close()
    print(f"Usunięto stare dane dla feed_id={feed_id}")



def load_all(feed_id, zip_path):
    import zipfile

    with zipfile.ZipFile(zip_path, "r") as z:
        if "feed_info.txt" in z.namelist():
            with z.open("feed_info.txt") as f:
                load_feed_info(feed_id, f)

        with z.open("agency.txt") as f:
            load_agency(feed_id, f)

        with z.open("stops.txt") as f:
            load_stops(feed_id, f)

        with z.open("routes.txt") as f:
            load_routes(feed_id, f)

        with z.open("calendar.txt") as f:
            load_calendar(feed_id, f)

        if "calendar_dates.txt" in z.namelist():
            with z.open("calendar_dates.txt") as f:
                load_calendar_dates(feed_id, f)

        with z.open("trips.txt") as f:
            load_trips(feed_id, f)

        with z.open("stop_times.txt") as f:
            load_stop_times(feed_id, f)
    print("Wczytano dane z pliku GTFS.")

def main():
    base_dir = os.path.dirname(os.path.abspath(__file__))
    folder = os.path.join(base_dir, "..", "GTFS-creator", "GotoweGTFS-y")
    print("Szukam folderu:", folder)

    gtfs_files = [f for f in os.listdir(folder) if f.endswith(".zip")]

    if not gtfs_files:
        print("Brak plików GTFS w folderze:", folder)
        return

    print("Dostępne pliki GTFS:")
    for i, file in enumerate(gtfs_files, start=1):
        print(f"{i}. {file}")

    choice = input("Wybierz numer pliku: ")
    try:
        idx = int(choice) - 1
        if idx < 0 or idx >= len(gtfs_files):
            print("Niepoprawny numer.")
            return
    except ValueError:
        print("Musisz podać liczbę.")
        return

    zip_path = os.path.join(folder, gtfs_files[idx])
    print(f"Wybrano: {zip_path}")
    feed_id = input("Podaj feed id: ")
    delete_feed(feed_id)
    load_all(feed_id, zip_path)
    

if __name__ == "__main__":
    main()
