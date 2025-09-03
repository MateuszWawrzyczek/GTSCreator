import pandas as pd
import io
import zipfile

excel_path = "C:/Users/matwa/Documents/Inzynierka/GTFS-creator/pre-GTFS/GTFS_Drabas.xlsx"
static_sheets = ["agency", "stops", "calendar", "calendar_dates", "feed_info", "routes"]
xlsx_raw = pd.read_excel(excel_path, sheet_name=None, header=None)

xlsx = {}
for sheet_name, df in xlsx_raw.items():
    if sheet_name.lower() in static_sheets:
        xlsx[sheet_name] = pd.read_excel(excel_path, sheet_name=sheet_name)  # z nagłówkami
    else:
        xlsx[sheet_name] = df

def parse_sheet_name(sheet_name):
    for prefix in ["weekdays", "saturday", "sunday", "holidays", "days", "vacation", "monsat", "weekends"]:
        if sheet_name.lower().startswith(prefix):
            # np. days_KTW_E_0 → prefix=days, rest="KTW_E_0"
            rest = sheet_name[len(prefix):].strip("_")
            parts = rest.split("_")
            if len(parts) >= 2:
                route_id = "_".join(parts[:-1])  # wszystko oprócz ostatniego = route_id
                direction_id = parts[-1]        # ostatni element = direction_id
            else:
                route_id = rest
                direction_id = "0"
            return prefix.lower(), route_id, int(direction_id)
    return None, None, None

trips_data = []
stop_times_data = []
routes = set()

trip_id_counter = 1  

for sheet_name, df in xlsx.items():
    day_type, route_id, direction_id = parse_sheet_name(sheet_name)
    if day_type is None:
        continue

    service_id = day_type.upper()

    # Pobierz nazwę linii z arkusza routes
    try:
        route_row = xlsx["routes"].loc[xlsx["routes"]["route_id"] == route_id].iloc[0]
        route_long_name = route_row["route_long_name"]
    except:
        route_long_name = f"Linia {route_id}"

    # każda kolumna z godzinami = osobny kurs
    for col_idx, column in enumerate(df.columns[2:], start=1):
        trip_id = f"t{trip_id_counter}"
        trip_id_counter += 1

        # pobierz kierunek z ostatniego wiersza tej kolumny
        headsign = str(df[column].iloc[-1])

        trips_data.append({
            "route_id": route_id,
            "service_id": service_id,
            "trip_id": trip_id,
            "trip_headsign": headsign,
            "direction_id": direction_id
        })

        # iteracja po przystankach, ale bez ostatniego wiersza (bo tam jest headsign)
        for stop_sequence, row in df.iloc[:-1].iterrows():
            time_value = row[column]
            if pd.isna(time_value):
                continue
            stop_times_data.append({
                "trip_id": trip_id,
                "arrival_time": str(time_value),
                "departure_time": str(time_value),
                "stop_id": str(row[0]).split('.')[0],
                "stop_sequence": stop_sequence + 1
            })

gtfs_frames = {
    "agency.txt": xlsx["agency"],
    "stops.txt": xlsx["stops"].drop(columns=["direction_desc"], errors="ignore"),
    "calendar.txt": xlsx["calendar"],
    "calendar_dates.txt": xlsx["calendar_dates"],
    "routes.txt": xlsx["routes"],
    "trips.txt": pd.DataFrame(trips_data),
    "stop_times.txt": pd.DataFrame(stop_times_data),
    "feed_info.txt": xlsx["feed_info"]
}

# poprawka daty w calendar_dates.txt
if "calendar_dates" in xlsx:
    df = gtfs_frames["calendar_dates.txt"].copy()
    df["date"] = pd.to_datetime(df["date"], errors="coerce")
    df["date"] = df["date"].dt.strftime("%Y%m%d")
    gtfs_frames["calendar_dates.txt"] = df

# zapis do zip
with zipfile.ZipFile("C:\\Users\\matwa\\Documents\\Inzynierka\\GTFS-creator\\gtfs.zip", "w", zipfile.ZIP_DEFLATED) as zf:
    for name, df in gtfs_frames.items():
        buffer = io.StringIO()
        df.to_csv(buffer, index=False)
        zf.writestr(name, buffer.getvalue())

print("✔️ Plik gtfs.zip został wygenerowany.")
