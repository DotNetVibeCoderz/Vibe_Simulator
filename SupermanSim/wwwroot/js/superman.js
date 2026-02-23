let viewer;
let speed = 0;
let maxSpeed = 3000;
let isSimulating = false;
let audioCtx;
let windNode, gainNode;

// Menambahkan penangkap event untuk arrowup dan arrowdown
const keys = { w: false, a: false, s: false, d: false, arrowup: false, arrowdown: false, shift: false, q: false, e: false };

window.initSimulator = async (mode, googleMapsApiKey) => {
    const container = document.getElementById("sim-container");
    if (!container) return;

    isSimulating = true;
    
    // Inisialisasi Cesium Viewer (Menggunakan Ion default untuk terrain jika API Google gagal)
    Cesium.Ion.defaultAccessToken = ""; // Jika kosong, beberapa default Cesium mungkin gagal/gelap (401 Error)
    
    // Untuk menghindari error 401 saat load WorldTerrain bawaan Cesium Ion (jika token tidak diset),
    // kita akan menggunakan EllipsoidTerrainProvider yang sepenuhnya gratis dan lokal.
    const terrainProvider = new Cesium.EllipsoidTerrainProvider();

    viewer = new Cesium.Viewer('sim-container', {
        baseLayerPicker: false,
        homeButton: false,
        infoBox: false,
        navigationHelpButton: false,
        sceneModePicker: false,
        geocoder: false,
        timeline: false,
        animation: false,
        fullscreenButton: false,
        selectionIndicator: false,
        terrainProvider: terrainProvider, // Menggunakan provider yang pasti tidak butuh token
        // Gunakan OpenStreetMap sebagai Imagery Provider gratis agar bumi terlihat bertekstur peta
        imageryProvider: new Cesium.OpenStreetMapImageryProvider({
            url: 'https://a.tile.openstreetmap.org/'
        })
    });

    // Menghilangkan logo Cesium di bawah
    viewer.cesiumWidget.creditContainer.style.display = "none";

    // =========================================================================
    // INTEGRASI GOOGLE MAPS PHOTOREALISTIC 3D TILES
    // =========================================================================
    if (googleMapsApiKey && googleMapsApiKey.trim() !== "") {
        try {
            // Memuat Google 3D Tiles
            const tileset = await Cesium.Cesium3DTileset.fromUrl(
                `https://tile.googleapis.com/v1/3dtiles/root.json?key=${googleMapsApiKey}`
            );
            viewer.scene.primitives.add(tileset);
            
            // Sembunyikan globe dasar agar tidak z-fighting dengan 3D tiles google
            viewer.scene.globe.show = false;
            
            console.log("Google 3D Tiles Berhasil Dimuat!");
        } catch (error) {
            console.error("Gagal memuat Google 3D Tiles. Cek API Key Anda.", error);
            viewer.scene.globe.show = true;
        }
    } else {
        console.warn("API Key Google Maps tidak diset di appsettings.json. Menggunakan Fallback Map (OpenStreetMap).");
        viewer.scene.globe.show = true;
        
        // Coba tambahkan bangunan 3D OSM bawaan (mungkin butuh token Ion, kita tangkap errornya jika gagal)
        try {
            const buildings = await Cesium.createOsmBuildingsAsync();
            viewer.scene.primitives.add(buildings);
        } catch(e) {
            console.warn("Gagal load OSM Buildings (biasanya karena butuh Cesium Ion Token), mengabaikan 3D buildings fallback.", e);
        }
    }

    // Mengatur posisi awal kamera
    let lon = -74.018813; // New York Default
    let lat = 40.691142;
    let alt = 1000.0;

    if (mode === 'tokyo_mission') {
        lon = 139.767125; lat = 35.681236; alt = 1500.0; // Tokyo
    } else if (mode === 'paris_mission') {
        lon = 2.294481; lat = 48.858370; alt = 500.0; // Paris Eiffel
    } else if (mode === 'supersonic') {
        alt = 10000.0; 
    }

    viewer.camera.setView({
        destination: Cesium.Cartesian3.fromDegrees(lon, lat, alt),
        orientation: {
            heading: Cesium.Math.toRadians(20.0),
            pitch: Cesium.Math.toRadians(-20.0), 
            roll: 0.0                           
        }
    });

    initAudio();
    setupControls();

    viewer.clock.onTick.addEventListener(updatePhysics);
    document.body.style.overflow = "hidden";
};

function setupControls() {
    document.addEventListener('keydown', handleKeyDown);
    document.addEventListener('keyup', handleKeyUp);
}

function handleKeyDown(e) {
    const k = e.key.toLowerCase();
    
    // Mencegah default scrolling dari browser saat menekan arrow up / down
    if (k === 'arrowup' || k === 'arrowdown') {
        e.preventDefault();
    }
    
    if(keys.hasOwnProperty(k)) keys[k] = true;
    
    // Perbaikan bug tombol Shift
    // key dari event untuk Shift adalah 'shift', bukan 'Shift' jika kita menggunakan toLowerCase()
    if(k === 'shift') keys.shift = true; 
}

function handleKeyUp(e) {
    const k = e.key.toLowerCase();
    
    if(keys.hasOwnProperty(k)) keys[k] = false;
    
    // Perbaikan bug tombol Shift saat dilepas
    if(k === 'shift') keys.shift = false;
}

function initAudio() {
    const AudioContext = window.AudioContext || window.webkitAudioContext;
    audioCtx = new AudioContext();
    const bufferSize = audioCtx.sampleRate * 2;
    const buffer = audioCtx.createBuffer(1, bufferSize, audioCtx.sampleRate);
    const output = buffer.getChannelData(0);
    for (let i = 0; i < bufferSize; i++) {
        output[i] = Math.random() * 2 - 1; 
    }
    
    windNode = audioCtx.createBufferSource();
    windNode.buffer = buffer;
    windNode.loop = true;
    
    const filter = audioCtx.createBiquadFilter();
    filter.type = 'lowpass';
    filter.frequency.value = 400;
    
    gainNode = audioCtx.createGain();
    gainNode.gain.value = 0;
    
    windNode.connect(filter);
    filter.connect(gainNode);
    gainNode.connect(audioCtx.destination);
    
    windNode.start();
}

function updatePhysics(clock) {
    if (!isSimulating || !viewer) return;

    const camera = viewer.camera;

    if (keys.w) {
        speed += 10;
    } else if (keys.s) {
        speed -= 10;
    } else {
        speed *= 0.96; 
    }
    
    // Turbo Boost logic yang sudah di fix
    // Jika tombol shift ditekan, kita memberikan akselerasi tambahan yang drastis
    if (keys.shift) {
        speed += 50; 
    }
    
    speed = Math.max(-1000, Math.min(speed, maxSpeed));

    if (Math.abs(speed) > 0.1) {
        camera.moveForward(speed * 0.1); 
    }

    if (keys.a) camera.lookLeft(0.02);
    if (keys.d) camera.lookRight(0.02);
    if (keys.q) camera.twistLeft(0.03);
    if (keys.e) camera.twistRight(0.03);
    
    // Pitch Up / Down menggunakan tombol Arrow
    if (keys.arrowup) camera.lookUp(0.02); 
    if (keys.arrowdown) camera.lookDown(0.02);   
    
    const cartographic = Cesium.Cartographic.fromCartesian(camera.position);
    if (cartographic && cartographic.height < 10) {
        cartographic.height = 10;
        camera.position = Cesium.Cartesian3.fromRadians(cartographic.longitude, cartographic.latitude, cartographic.height);
    }

    if (audioCtx && audioCtx.state === 'running') {
        const speedRatio = Math.abs(speed) / maxSpeed;
        gainNode.gain.value = speedRatio * 0.8;
    }

    const hudSpeed = document.getElementById("hud-speed");
    const hudAlt = document.getElementById("hud-alt");
    if(hudSpeed) hudSpeed.innerText = Math.round(Math.abs(speed)*1.5) + " KM/H";
    if(hudAlt && cartographic) hudAlt.innerText = Math.round(cartographic.height) + " M";
}

window.startAudio = () => {
    if (audioCtx && audioCtx.state !== 'running') {
        audioCtx.resume();
    }
};

window.disposeSimulator = () => {
    isSimulating = false;
    document.removeEventListener('keydown', handleKeyDown);
    document.removeEventListener('keyup', handleKeyUp);
    
    if (viewer) {
        viewer.clock.onTick.removeEventListener(updatePhysics);
        viewer.destroy();
        viewer = null;
    }
    if (audioCtx) {
        audioCtx.close();
    }
    const container = document.getElementById("sim-container");
    if (container) container.innerHTML = '';
    document.body.style.overflow = "auto";
};
