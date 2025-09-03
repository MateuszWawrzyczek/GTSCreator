import pandas as pd
from db import get_connection

def load_calendar(feed_id: str, filepath: str):
    conn = get_connection()
    cur = conn.cursor()

    df = pd.read_csv(filepath)

    for _, row in df.iterrows():
        cur.execute("""
            INSERT INTO calendar (feed_id, service_id, monday, tuesday, wednesday, thursday, friday, saturday, sunday, start_date, end_date)
            VALUES (%s, %s, %s, %s, %s, %s, %s, %s, %s, %s, %s)
            ON CONFLICT (feed_id, service_id) DO NOTHING;
        """, (
            feed_id,
            row["service_id"],
            row["monday"],
            row["tuesday"],
            row["wednesday"],
            row["thursday"],
            row["friday"],
            row["saturday"],
            row["sunday"],
            pd.to_datetime(row["start_date"], format="%Y%m%d").date(),
            pd.to_datetime(row["end_date"], format="%Y%m%d").date()
        ))

    conn.commit()
    cur.close()
    conn.close()
