import pandas as pd
from db import get_connection

def load_stop_times(feed_id: str, filepath: str):
    conn = get_connection()
    cur = conn.cursor()

    df = pd.read_csv(filepath)

    for _, row in df.iterrows():
        cur.execute("""
            INSERT INTO stop_times (feed_id, trip_id, arrival_time, departure_time, stop_id, stop_sequence )
            VALUES (%s, %s, %s, %s, %s, %s)
             ON CONFLICT (feed_id, trip_id, stop_sequence) DO NOTHING;
        """, (
            feed_id,
            row["trip_id"],
            row["arrival_time"],
            row["departure_time"],
            row["stop_id"],
            int(row["stop_sequence"])
        ))

    conn.commit()
    cur.close()
    conn.close()
