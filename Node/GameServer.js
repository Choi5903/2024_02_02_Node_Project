const express = require('express');
const mysql = require('mysql2/promise');
const app = express();

app.use(express.json());

//MYSQL 연결 설정
const pool = mysql.createPool({
    host : 'localhost',
    user : 'root',
    password : 'shingu',
    database : 'game_world'
});

//플레이어 로그인
app.post('/login' , async (req, res) => {
    const { username, password_hash} = req.body;   
    try
    {
        const [players] = await pool.query(
            
            `SELECT * FROM players WHERE username = ? AND password_hash = ?`,
            [username, password_hash]
        );
        if (players.length > 0)
        {
            await pool.query(
                'UPDATE players SET last_login = CURRENT_TIMESTAMP WHERE player_id = ?',
                [players[0].player_id]
            );
            res.json({success: true, player:players[0]});
        }
        else
        {
            res.status(401).json({ success: false, message : '로그인 실패'});
        }
    }
    catch (error)
    {
       res.status(500).json({ success: false , message : error.message});
    }
});

//플레이어 인벤토리 조회
app.get('/inventory/:playerId', async (req, res) => {

    try
    {
        const[inventory] = await pool.query(
            'SELECT i.* , inv.quantity FROM inventories inv JOIN items i ON inv.item_id = i.item_id WHERE inv.player_id = ?',
            [req.params.playerId]
        );
        res.json(inventory);
    }
    catch(error)
    {
        res.status(500).json({ success: false , message : error.message});
    }
});

//퀘스트 목록 조회
app.get('/quests/:playerId' , async (req, res) => {
    try
    {
        const[quests] = await pool.query(
            'SELECT q.* , pq.status FROM player_quests pq JOIN quests q ON pq.quest_id = q.quest_id WHERE pq.player_id = ?',
            [req.params.playerId]
        );
        res.json(quests);
    }
    catch(error)
    {
        res.status(500).json({ success: false , message : error.message});
    } 
});

//퀘스트 상태 업데이트 
app.get('/quests/status' , async (req, res) => {
    const {playerId, questId, status} = req.body;
    try
    {
        await pool.query(
            `UPDATE player_quests SET status = ? , complete_at = IF(? = "완료", CURRENT_TIMESTAMP, null) WERHE player_id = ?
            AND quest_id = ?`,
            [status, status, playerId ,questId ]
        );
        res.json({success: true});
    }
    catch(error)
    {
        res.status(500).json({ success: false , message : error.message});
    } 
});

//아이템 획득
app.post('/inventory/add', async (req, res) => {
    const {playerId, itemId, quantity} = req.body;
    try
    {    //기존 인벤토리 확인
        const [existing] = await pool.query(
            `SELECT * FROM inventories WHERE player_id = ? AND item_id = ?`,
            [playerId, itemId]
        );   

        if (existing.length > 0 )   //기존 아이템 수량 업데이트 
        {
            await pool.query(
                `UPDATE inventories SET quantity = quantity + ? WHERE player_id = ? AND item_id = ?`,
                [quantity, playerId, itemId]
            );
        }
        else 
        {
            await pool.query(
                `INSERT INTO inventories (player_id, item_id, quantity) VALUES (?,?,?)`,
                [playerId, itemId, quantity]
            );
        }
        res.json({ success: true});
    }
    catch(error)
    {
        res.status(500).json({ success: false , message : error.message});
    } 
});

const PORT = 3000;

app.listen(PORT, () => {
    console.log(`서버 실행 중 : ${PORT}`);
});