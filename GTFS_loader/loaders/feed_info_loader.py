import pandas as pd
from db import get_connection

def load_feed_info(feed_id: str, filepath: str):
    conn = get_connection()
    cur = conn.cursor()

    df = pd.read_csv(filepath)

    for _, row in df.iterrows():
        cur.execute("""
            INSERT INTO feed_info (feed_id, feed_publisher_name, feed_publisher_url, feed_lang, feed_start_date, feed_end_date)
            VALUES (%s, %s, %s, %s, %s, %s)
            ON CONFLICT (feed_id) DO NOTHING;
        """, (
            feed_id,
            row["feed_publisher_name"],
            row["feed_publisher_url"],
            row["feed_lang"],
            pd.to_datetime(row["feed_start_date"], format="%Y%m%d").date(),
            pd.to_datetime(row["feed_end_date"], format="%Y%m%d").date()
        ))

    conn.commit()
    cur.close()
    conn.close()
