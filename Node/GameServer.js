const express = require('express');
const mysql = require('mysql2/promise');
const app = express();

// JSON 요청 바디 파싱
app.use(express.json());

// MySQL 연결 설정
const pool = mysql.createPool({
    host: 'localhost',
    user: 'root',
    password: 'shingu',
    database: 'game_world'
});

// 요청 디버깅 미들웨어
app.use((req, res, next) => {
    console.log(`[Request] ${req.method} ${req.url}`);
    if (req.body && Object.keys(req.body).length > 0) {
        console.log('[Request Body]', req.body);
    }
    next();
});

// 플레이어 로그인
app.post('/login', async (req, res) => {
    const { username, password_hash } = req.body;

    try {
        console.log('[Login] Attempting login for user:', username);

        const [players] = await pool.query(
            'SELECT * FROM players WHERE username = ? AND password_hash = ?',
            [username, password_hash]
        );

        console.log('[Login] Query Result:', players);

        if (players.length > 0) {
            const playerId = players[0].player_id;

            console.log('[Login] Successful login. Updating last_login for player_id:', playerId);

            await pool.query(
                'UPDATE players SET last_login = CURRENT_TIMESTAMP WHERE player_id = ?',
                [playerId]
            );

            res.json({ success: true, player: players[0] });
        } else {
            console.warn('[Login] Login failed for user:', username);
            res.status(401).json({ success: false, message: '로그인 실패' });
        }
    } catch (error) {
        console.error('[Login Error]', error);
        res.status(500).json({ success: false, message: error.message });
    }
});

// 플레이어 인벤토리 조회
app.get('/inventory/:playerId', async (req, res) => {
    const playerId = req.params.playerId;

    try {
        console.log('[Inventory] Fetching inventory for player_id:', playerId);

        const [inventory] = await pool.query(
            'SELECT i.*, inv.quantity FROM inventories inv JOIN items i ON inv.item_id = i.item_id WHERE inv.player_id = ?',
            [playerId]
        );

        console.log('[Inventory] Query Result:', inventory);

        res.json(inventory);
    } catch (error) {
        console.error('[Inventory Error]', error);
        res.status(500).json({ success: false, message: error.message });
    }
});

// 퀘스트 목록 조회
app.get('/quests/:playerId', async (req, res) => {
    const playerId = req.params.playerId;

    try {
        console.log('[Quests] Fetching quests for player_id:', playerId);

        const [quests] = await pool.query(
            'SELECT q.*, pq.status FROM player_quests pq JOIN quests q ON pq.quest_id = q.quest_id WHERE pq.player_id = ?',
            [playerId]
        );

        console.log('[Quests] Query Result:', quests);

        res.json(quests);
    } catch (error) {
        console.error('[Quests Error]', error);
        res.status(500).json({ success: false, message: error.message });
    }
});

// 퀘스트 상태 업데이트
app.post('/quests/status', async (req, res) => {
    const { playerId, questId, status } = req.body;

    try {
        console.log('[Quest Update] Updating status for quest_id:', questId, 'player_id:', playerId);

        await pool.query(
            'UPDATE player_quests SET status = ?, complete_at = IF(? = "완료", CURRENT_TIMESTAMP, null) WHERE player_id = ? AND quest_id = ?',
            [status, status, playerId, questId]
        );

        console.log('[Quest Update] Status updated successfully');

        res.json({ success: true });
    } catch (error) {
        console.error('[Quest Update Error]', error);
        res.status(500).json({ success: false, message: error.message });
    }
});

// 아이템 획득
app.post('/inventory/add', async (req, res) => {
    const { playerId, itemId, quantity } = req.body;

    try {
        console.log('[Inventory Add] Adding item:', itemId, 'to player_id:', playerId, 'quantity:', quantity);

        // 기존 인벤토리 확인
        const [existing] = await pool.query(
            'SELECT * FROM inventories WHERE player_id = ? AND item_id = ?',
            [playerId, itemId]
        );

        console.log('[Inventory Add] Existing inventory:', existing);

        if (existing.length > 0) {
            console.log('[Inventory Add] Updating existing inventory quantity');

            await pool.query(
                'UPDATE inventories SET quantity = quantity + ? WHERE player_id = ? AND item_id = ?',
                [quantity, playerId, itemId]
            );
        } else {
            console.log('[Inventory Add] Adding new item to inventory');

            await pool.query(
                'INSERT INTO inventories (player_id, item_id, quantity) VALUES (?,?,?)',
                [playerId, itemId, quantity]
            );
        }

        res.json({ success: true });
    } catch (error) {
        console.error('[Inventory Add Error]', error);
        res.status(500).json({ success: false, message: error.message });
    }
});

// 서버 시작
const PORT = 3000;
app.listen(PORT, () => {
    console.log(`서버 실행 중 : ${PORT}`);
});
