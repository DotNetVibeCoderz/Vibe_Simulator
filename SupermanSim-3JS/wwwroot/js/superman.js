let scene, camera, renderer;
let speed = 0;
let maxSpeed = 3000;
let isSimulating = false;
let audioCtx;
let windNode, gainNode;

const keys = { w: false, a: false, s: false, d: false, space: false, shift: false, q: false, e: false };

window.initSimulator = (mode) => {
    const container = document.getElementById("sim-container");
    if (!container) return;

    isSimulating = true;
    scene = new THREE.Scene();
    scene.background = new THREE.Color(0x87CEEB); 
    scene.fog = new THREE.Fog(0x87CEEB, 1000, 20000);

    camera = new THREE.PerspectiveCamera(75, window.innerWidth / window.innerHeight, 0.1, 50000);
    camera.position.set(0, 1000, 1000);

    renderer = new THREE.WebGLRenderer({ antialias: true, alpha: false });
    renderer.setSize(window.innerWidth, window.innerHeight);
    container.appendChild(renderer.domElement);

    const dirLight = new THREE.DirectionalLight(0xffffff, 1);
    dirLight.position.set(5000, 5000, 5000);
    scene.add(dirLight);
    scene.add(new THREE.AmbientLight(0x555555));

    // Ground Plane
    const earthGeo = new THREE.PlaneGeometry(200000, 200000, 100, 100);
    const earthMat = new THREE.MeshLambertMaterial({ color: 0x1B4D3E, wireframe: false }); 
    const earth = new THREE.Mesh(earthGeo, earthMat);
    earth.rotation.x = -Math.PI / 2;
    scene.add(earth);

    // Grid (Brutalist style)
    const gridHelper = new THREE.GridHelper(200000, 500, 0x000000, 0x000000);
    scene.add(gridHelper);

    // Generate random brutalist blocks for cities
    const cityMat = new THREE.MeshLambertMaterial({ color: 0x888888 });
    const buildingGeo = new THREE.BoxGeometry(200, 1, 200);

    const numCities = mode === 'city_tour' ? 100 : 30;
    
    for (let c = 0; c < numCities; c++) {
        const cx = (Math.random() - 0.5) * 80000;
        const cz = (Math.random() - 0.5) * 80000;
        
        const cityGroup = new THREE.Group();
        cityGroup.position.set(cx, 0, cz);

        for (let i = 0; i < 40; i++) {
            const height = Math.random() * 2000 + 500;
            const mesh = new THREE.Mesh(buildingGeo, cityMat);
            mesh.scale.y = height;
            mesh.position.set(
                (Math.random() - 0.5) * 4000,
                height / 2,
                (Math.random() - 0.5) * 4000
            );
            cityGroup.add(mesh);
        }
        scene.add(cityGroup);
    }

    // Clouds
    const cloudGeo = new THREE.BoxGeometry(500, 200, 500);
    const cloudMat = new THREE.MeshLambertMaterial({ color: 0xffffff, transparent: true, opacity: 0.8 });
    for (let i = 0; i < 200; i++) {
        const cloud = new THREE.Mesh(cloudGeo, cloudMat);
        cloud.position.set(
            (Math.random() - 0.5) * 100000,
            Math.random() * 5000 + 2000,
            (Math.random() - 0.5) * 100000
        );
        scene.add(cloud);
    }

    initAudio();
    setupControls();

    window.addEventListener('resize', onWindowResize, false);
    document.body.style.overflow = "hidden"; // Prevent scrolling
    
    animate();
};

function setupControls() {
    document.addEventListener('keydown', handleKeyDown);
    document.addEventListener('keyup', handleKeyUp);
}

function handleKeyDown(e) {
    const k = e.key.toLowerCase();
    if(keys.hasOwnProperty(k)) keys[k] = true;
    if(e.key === 'Shift') keys.shift = true;
    if(e.key === ' ') keys.space = true;
}

function handleKeyUp(e) {
    const k = e.key.toLowerCase();
    if(keys.hasOwnProperty(k)) keys[k] = false;
    if(e.key === 'Shift') keys.shift = false;
    if(e.key === ' ') keys.space = false;
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

function updatePhysics() {
    if (keys.w) {
        speed += 10;
    } else if (keys.s) {
        speed -= 10;
    } else {
        speed *= 0.98;
    }
    
    if (keys.shift) {
        speed += 20;
    }
    
    speed = Math.max(-500, Math.min(speed, maxSpeed));

    const dir = new THREE.Vector3(0, 0, -1);
    dir.applyQuaternion(camera.quaternion);
    dir.multiplyScalar(speed);
    camera.position.add(dir);

    // Mouse or keyboard rotation
    if (keys.a) camera.rotateY(0.02);
    if (keys.d) camera.rotateY(-0.02);
    if (keys.q) camera.rotateZ(0.03);
    if (keys.e) camera.rotateZ(-0.03);
    
    // Pitch up/down manually
    if (keys.space) camera.rotateX(0.02);
    // Let's add 'C' or something to pitch down? No, just point with mouse if possible, but let's stick to simple keyboard: w/s = forward/backward, a/d = yaw, q/e = roll, space = pitch up, maybe Shift+Space = pitch down?
    // Simplify: space to pitch up, 'z' or something to pitch down, but we didn't track z.
    
    if (camera.position.y < 50) camera.position.y = 50;
    
    if (audioCtx && audioCtx.state === 'running') {
        const speedRatio = Math.abs(speed) / maxSpeed;
        gainNode.gain.value = speedRatio * 0.8;
    }

    const hudSpeed = document.getElementById("hud-speed");
    const hudAlt = document.getElementById("hud-alt");
    if(hudSpeed) hudSpeed.innerText = Math.round(Math.abs(speed)*1.5) + " KM/H";
    if(hudAlt) hudAlt.innerText = Math.round(camera.position.y) + " M";
}

function onWindowResize() {
    if (!camera || !renderer) return;
    camera.aspect = window.innerWidth / window.innerHeight;
    camera.updateProjectionMatrix();
    renderer.setSize(window.innerWidth, window.innerHeight);
}

function animate() {
    if (!isSimulating) return;
    requestAnimationFrame(animate);
    updatePhysics();
    renderer.render(scene, camera);
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
    if (renderer) {
        renderer.dispose();
    }
    if (audioCtx) {
        audioCtx.close();
    }
    const container = document.getElementById("sim-container");
    if (container) container.innerHTML = '';
    document.body.style.overflow = "auto";
};
