// GoldenBridgeSim Engine
// Powered by Three.js
// Gravicode Studios - Jacky the Code Bender

let scene, camera, renderer;
let vehicles = [];
let roadWidth = 20;
let bridgeLength = 400;
let animationId;
let sunLight, ambientLight;
let isRaining = false;
let rainSystem;

// Camera Control Variables
let moveForward = false;
let moveBackward = false;
let moveLeft = false;
let moveRight = false;
let moveUp = false; 
let moveDown = false;

let isDragging = false;
let previousMousePosition = { x: 0, y: 0 };
const rotationSpeed = 0.002;
const movementSpeed = 0.5;

// Configuration exposed to C#
let config = {
    trafficSpeed: 0.5,
    isNight: false,
    density: 50 // chance to spawn
};

export function initSim(canvasId) {
    const container = document.getElementById(canvasId);
    
    // 1. Scene Setup
    scene = new THREE.Scene();
    scene.background = new THREE.Color(0x87CEEB); // Sky blue default
    scene.fog = new THREE.FogExp2(0x87CEEB, 0.002);

    // 2. Camera
    camera = new THREE.PerspectiveCamera(60, container.clientWidth / container.clientHeight, 0.1, 1000);
    camera.position.set(0, 30, 60);
    camera.lookAt(0, 0, 0);
    camera.rotation.order = 'YXZ'; // Important for first-person feel

    // 3. Renderer
    renderer = new THREE.WebGLRenderer({ antialias: true });
    renderer.setSize(container.clientWidth, container.clientHeight);
    renderer.shadowMap.enabled = true;
    container.appendChild(renderer.domElement);

    // 4. Lighting
    ambientLight = new THREE.AmbientLight(0xffffff, 0.6);
    scene.add(ambientLight);

    sunLight = new THREE.DirectionalLight(0xffffff, 1);
    sunLight.position.set(50, 100, 50);
    sunLight.castShadow = true;
    sunLight.shadow.camera.top = 50;
    sunLight.shadow.camera.bottom = -50;
    sunLight.shadow.camera.left = -50;
    sunLight.shadow.camera.right = 50;
    scene.add(sunLight);

    // 5. Build Environment
    buildBridge();
    buildEnvironment();

    // 6. Handle Events
    window.addEventListener('resize', onWindowResize, false);
    
    // Input Listeners for Camera
    document.addEventListener('keydown', onKeyDown);
    document.addEventListener('keyup', onKeyUp);
    // Attach mouse events to the renderer's DOM element
    renderer.domElement.addEventListener('mousedown', onMouseDown);
    document.addEventListener('mouseup', onMouseUp); 
    document.addEventListener('mousemove', onMouseMove);

    // 7. Start Loop
    animate();
}

// ... helper functions for input ...
function onKeyDown(event) {
    switch (event.code) {
        case 'ArrowUp':
        case 'KeyW': moveForward = true; break;
        case 'ArrowLeft':
        case 'KeyA': moveLeft = true; break;
        case 'ArrowDown':
        case 'KeyS': moveBackward = true; break;
        case 'ArrowRight':
        case 'KeyD': moveRight = true; break;
        case 'Space': moveUp = true; break;
        case 'ShiftLeft':
        case 'ShiftRight': moveDown = true; break;
    }
}

function onKeyUp(event) {
    switch (event.code) {
        case 'ArrowUp':
        case 'KeyW': moveForward = false; break;
        case 'ArrowLeft':
        case 'KeyA': moveLeft = false; break;
        case 'ArrowDown':
        case 'KeyS': moveBackward = false; break;
        case 'ArrowRight':
        case 'KeyD': moveRight = false; break;
        case 'Space': moveUp = false; break;
        case 'ShiftLeft':
        case 'ShiftRight': moveDown = false; break;
    }
}

function onMouseDown(e) {
    isDragging = true;
    previousMousePosition = { x: e.clientX, y: e.clientY };
}

function onMouseUp(e) {
    isDragging = false;
}

function onMouseMove(e) {
    if (!isDragging) return;
    
    const deltaX = e.clientX - previousMousePosition.x;
    const deltaY = e.clientY - previousMousePosition.y;

    camera.rotation.y -= deltaX * rotationSpeed;
    camera.rotation.x -= deltaY * rotationSpeed;
    
    // Clamp pitch
    camera.rotation.x = Math.max(-Math.PI/2, Math.min(Math.PI/2, camera.rotation.x));

    previousMousePosition = { x: e.clientX, y: e.clientY };
}

function buildBridge() {
    // Road
    const geometry = new THREE.BoxGeometry(roadWidth, 1, bridgeLength);
    const material = new THREE.MeshPhongMaterial({ color: 0x333333 }); 
    const road = new THREE.Mesh(geometry, material);
    road.receiveShadow = true;
    scene.add(road);

    // Lines
    const lineGeo = new THREE.BoxGeometry(0.5, 1.1, bridgeLength);
    const lineMat = new THREE.MeshBasicMaterial({ color: 0xffffff });
    const centerLine = new THREE.Mesh(lineGeo, lineMat);
    scene.add(centerLine);

    // Cables / Towers
    const towerGeo = new THREE.BoxGeometry(2, 40, 2);
    const towerMat = new THREE.MeshPhongMaterial({ color: 0xC0392B }); 
    
    const tower1 = new THREE.Mesh(towerGeo, towerMat);
    tower1.position.set(roadWidth/2 + 1, 20, -100);
    tower1.castShadow = true;
    scene.add(tower1);

    const tower2 = new THREE.Mesh(towerGeo, towerMat);
    tower2.position.set(-(roadWidth/2 + 1), 20, -100);
    tower2.castShadow = true;
    scene.add(tower2);

    const tower3 = new THREE.Mesh(towerGeo, towerMat);
    tower3.position.set(roadWidth/2 + 1, 20, 100);
    tower3.castShadow = true;
    scene.add(tower3);

    const tower4 = new THREE.Mesh(towerGeo, towerMat);
    tower4.position.set(-(roadWidth/2 + 1), 20, 100);
    tower4.castShadow = true;
    scene.add(tower4);

    const cableGeo = new THREE.CylinderGeometry(0.2, 0.2, 200);
    const cableMat = new THREE.MeshPhongMaterial({ color: 0xC0392B });
    
    const cableLeft = new THREE.Mesh(cableGeo, cableMat);
    cableLeft.rotation.x = Math.PI / 2;
    cableLeft.position.set(-(roadWidth/2 + 1), 40, 0);
    scene.add(cableLeft);

    const cableRight = new THREE.Mesh(cableGeo, cableMat);
    cableRight.rotation.x = Math.PI / 2;
    cableRight.position.set((roadWidth/2 + 1), 40, 0);
    scene.add(cableRight);
}

function buildEnvironment() {
    const waterGeo = new THREE.PlaneGeometry(1000, 1000);
    const waterMat = new THREE.MeshPhongMaterial({ color: 0x004488, transparent: true, opacity: 0.8 });
    const water = new THREE.Mesh(waterGeo, waterMat);
    water.rotation.x = -Math.PI / 2;
    water.position.y = -20;
    scene.add(water);
}

export function spawnVehicle() {
    const color = Math.random() * 0xffffff;
    const geo = new THREE.BoxGeometry(2, 1.5, 4);
    const mat = new THREE.MeshPhongMaterial({ color: color });
    const car = new THREE.Mesh(geo, mat);
    
    const isRightLane = Math.random() > 0.5;
    const xPos = isRightLane ? 5 : -5;
    const zStart = isRightLane ? -bridgeLength/2 : bridgeLength/2;
    
    car.position.set(xPos, 1.5, zStart);
    car.castShadow = true;
    
    car.userData = {
        direction: isRightLane ? 1 : -1,
        speed: (Math.random() * 0.2 + 0.3)
    };

    scene.add(car);
    vehicles.push(car);
}

export function setTimeOfDay(hour) {
    if (hour >= 18 || hour < 6) {
        config.isNight = true;
        scene.background = new THREE.Color(0x000022);
        scene.fog.color.setHex(0x000022);
        sunLight.intensity = 0.1;
        ambientLight.intensity = 0.2;
    } else {
        config.isNight = false;
        scene.background = new THREE.Color(0x87CEEB);
        scene.fog.color.setHex(0x87CEEB);
        sunLight.intensity = 1;
        ambientLight.intensity = 0.6;
    }
}

export function setWeather(type) {
    if (type === 'rain') {
        isRaining = true;
        createRain();
    } else {
        isRaining = false;
        if(rainSystem) {
            scene.remove(rainSystem);
            rainSystem = null;
        }
    }
}

function createRain() {
    const rainCount = 10000;
    const rainGeo = new THREE.BufferGeometry();
    const positions = [];
    
    for(let i=0; i<rainCount; i++) {
        positions.push(
            Math.random() * 400 - 200,
            Math.random() * 200,
            Math.random() * 400 - 200
        );
    }
    
    rainGeo.setAttribute('position', new THREE.Float32BufferAttribute(positions, 3));
    const rainMat = new THREE.PointsMaterial({
        color: 0xaaaaaa,
        size: 0.5,
        transparent: true
    });
    
    rainSystem = new THREE.Points(rainGeo, rainMat);
    scene.add(rainSystem);
}

function animate() {
    requestAnimationFrame(animate);
    
    // Camera Logic
    if (moveForward) camera.translateZ(-movementSpeed);
    if (moveBackward) camera.translateZ(movementSpeed);
    if (moveLeft) camera.translateX(-movementSpeed);
    if (moveRight) camera.translateX(movementSpeed);
    if (moveUp) camera.translateY(movementSpeed);
    if (moveDown) camera.translateY(-movementSpeed);

    // Vehicle Logic
    for (let i = vehicles.length - 1; i >= 0; i--) {
        const car = vehicles[i];
        const dir = car.userData.direction;
        const speed = car.userData.speed * config.trafficSpeed;
        
        car.position.z += speed * dir;

        if (Math.abs(car.position.z) > bridgeLength / 2 + 20) {
            scene.remove(car);
            vehicles.splice(i, 1);
        }
    }

    // Rain Logic
    if (isRaining && rainSystem) {
        const positions = rainSystem.geometry.attributes.position.array;
        for(let i=1; i<positions.length; i+=3) {
            positions[i] -= 2;
            if (positions[i] < -20) {
                positions[i] = 100;
            }
        }
        rainSystem.geometry.attributes.position.needsUpdate = true;
    }

    // Auto Spawn Loop
    if (Math.random() * 100 < 2) { 
        spawnVehicle();
    }

    renderer.render(scene, camera);
}

function onWindowResize() {
    const container = renderer.domElement.parentElement;
    camera.aspect = container.clientWidth / container.clientHeight;
    camera.updateProjectionMatrix();
    renderer.setSize(container.clientWidth, container.clientHeight);
}

export function updateSpeed(val) {
    config.trafficSpeed = val;
}