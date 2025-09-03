import pandas as pd
from db import get_connection

def load_calendar_dates(feed_id: str, filepath: str):
    conn = get_connection()
    cur = conn.cursor()

    df = pd.read_csv(filepath)

    for _, row in df.iterrows():
        cur.execute("""
            INSERT INTO calendar_dates (feed_id, service_id, date, exception_type)
            VALUES (%s, %s, %s, %s)
            ON CONFLICT (feed_id, service_id, date) DO NOTHING;
        """, (
            feed_id,
            row["service_id"],
            pd.to_datetime(row["date"], format="%Y%m%d").date(),
            row["exception_type"]
        ))

    conn.commit()
    cur.close()
    conn.close()
