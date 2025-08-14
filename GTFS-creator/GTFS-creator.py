import pandas as pd
import io
import zipfile

excel_path = "C:/Users/matwa/Documents/Inzynierka/GTFS-creator/pre-GTFS/GTFS_Swierklany.xlsx"
static_sheets = ["agency", "stops", "calendar", "calendar_dates", "feed_info"]
xlsx_raw = pd.read_excel(excel_path, sheet_name=None, header=None)

xlsx = {}
for sheet_name, df in xlsx_raw.items():
    if sheet_name.lower() in static_sheets:
        xlsx[sheet_name] = pd.read_excel(excel_path, sheet_name=sheet_name)  # z nagłówkami
    else:
        xlsx[sheet_name] = df

def parse_sheet_name(sheet_name):
    for prefix in ["weekdays", "saturday", "sunday", "holidays", "vacation"]:
        if sheet_name.lower().startswith(prefix):
            return prefix.lower(), sheet_name[len(prefix):]  
    return None, None

trips_data = []
stop_times_data = []
routes = set()

trip_id_counter = 1  

for sheet_name, df in xlsx.items():
    day_type, line_number = parse_sheet_name(sheet_name)
    if day_type is None:
        continue  

    route_id = line_number.strip()
    service_id = day_type.upper()  
    route_short_name = route_id

    if route_id not in routes:
        routes.add(route_id)

    for col_idx, column in enumerate(df.columns[2:], start=1):
        trip_id = f"t{trip_id_counter}"
        trip_id_counter += 1

        trips_data.append({
            "route_id": route_id,
            "service_id": service_id,
            "trip_id": trip_id,
            "trip_headsign": f"Linia {route_id}",
        })

        for stop_sequence, row in df.iterrows():
            time_value = row[column]
            if pd.isna(time_value):
                continue  
            stop_times_data.append({
                "trip_id": trip_id,
                "arrival_time": str(time_value),
                "departure_time": str(time_value),
                "stop_id": row[0], 
                "stop_sequence": stop_sequence + 1
            })

routes_df = pd.DataFrame([{
    "route_id": rid,
    "agency_id": xlsx["agency"].iloc[0]["agency_id"],
    "route_short_name": rid,
    "route_long_name": f"Linia {rid}",
    "route_type": 3  
} for rid in routes])

gtfs_frames = {
    "agency.txt": xlsx["agency"],
    "stops.txt": xlsx["stops"].drop(columns=["direction_desc"], errors="ignore"),
    "calendar.txt": xlsx["calendar"],
    "calendar_dates.txt": xlsx["calendar_dates"],
    "routes.txt": routes_df,
    "trips.txt": pd.DataFrame(trips_data),
    "stop_times.txt": pd.DataFrame(stop_times_data),
    "feed_info.txt": xlsx["feed_info"]
}

for name, df in xlsx.items():
    if name == "calendar_dates":
        df = gtfs_frames["calendar_dates.txt"].copy()
        df["date"] = pd.to_datetime(df["date"], errors="coerce")
        df["date"] = df["date"].dt.strftime("%Y%m%d")
        gtfs_frames["calendar_dates.txt"] = df

with zipfile.ZipFile("gtfs.zip", "w", zipfile.ZIP_DEFLATED) as zf:
    for name, df in gtfs_frames.items():
        buffer = io.StringIO()
        df.to_csv(buffer, index=False)
        zf.writestr(name, buffer.getvalue())

print("✔️ Plik gtfs.zip został wygenerowany.")
