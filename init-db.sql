-- Создаем таблицу для статистики событий
CREATE TABLE IF NOT EXISTS user_event_stats (
    user_id INT NOT NULL,
    event_type VARCHAR(50) NOT NULL,
    count INT NOT NULL,
    last_updated TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    PRIMARY KEY (user_id, event_type)
);

-- Создаем индекс для быстрого поиска
CREATE INDEX IF NOT EXISTS idx_user_event_stats_user_id ON user_event_stats(user_id);
CREATE INDEX IF NOT EXISTS idx_user_event_stats_event_type ON user_event_stats(event_type);

-- Вставляем тестовые данные 
INSERT INTO user_event_stats (user_id, event_type, count) VALUES 
(1, 'click', 5),
(2, 'hover', 3),
(3, 'submit', 2)
ON CONFLICT (user_id, event_type) DO NOTHING;