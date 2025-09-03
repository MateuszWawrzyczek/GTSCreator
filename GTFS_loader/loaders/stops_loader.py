import pandas as pd
from db import get_connection

def load_stops(feed_id: str, filepath: str):
    conn = get_connection()
    cur = conn.cursor()

    df = pd.read_csv(filepath)

    for _, row in df.iterrows():
        cur.execute("""
            INSERT INTO stops (feed_id, stop_id, stop_name, stop_code, stop_lon, stop_lat)
            VALUES (%s, %s, %s, %s, %s, %s)
            ON CONFLICT (feed_id, stop_id) DO NOTHING;
        """, (
            feed_id,
            row["stop_id"],
            row["stop_name"],
            row["stop_code"],
            row["stop_lon"],
            row["stop_lat"]
        ))

    conn.commit()
    cur.close()
    conn.close()
