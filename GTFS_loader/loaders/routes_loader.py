import pandas as pd
from db import get_connection

def load_routes(feed_id: str, filepath: str):
    conn = get_connection()
    cur = conn.cursor()

    df = pd.read_csv(filepath)

    for _, row in df.iterrows():
        cur.execute("""
            INSERT INTO routes (feed_id, route_id, agency_id, route_short_name, route_long_name, route_type)
            VALUES (%s, %s, %s, %s, %s, %s)
            ON CONFLICT (feed_id, route_id) DO NOTHING;
        """, (
            feed_id,
            row["route_id"],
            row["agency_id"],
            row["route_short_name"],
            row["route_long_name"],
            row["route_type"],
        ))

    conn.commit()
    cur.close()
    conn.close()
