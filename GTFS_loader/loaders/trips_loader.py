import pandas as pd
from db import get_connection

def load_trips(feed_id: str, filepath: str):
    conn = get_connection()
    cur = conn.cursor()

    df = pd.read_csv(filepath)

    for _, row in df.iterrows():
        cur.execute("""
            INSERT INTO trips (feed_id, route_id, service_id, trip_id, trip_headsign, direction_id)
            VALUES (%s, %s, %s, %s, %s, %s)
            ON CONFLICT (feed_id, trip_id) DO NOTHING;
        """, (
            feed_id,
            row["route_id"],
            row["service_id"],
            row["trip_id"],
            row.get("trip_headsign"),
            int(row["direction_id"]) if not pd.isna(row.get("direction_id")) else None
        ))

    conn.commit()
    cur.close()
    conn.close()
