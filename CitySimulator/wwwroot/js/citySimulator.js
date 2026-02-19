window.citySim = {
    scene: null,
    camera: null,
    renderer: null,
    controls: null,
    animationId: null,
    buildingsMesh: null,
    carsMesh: null,
    rainParticles: null,
    directionalLight: null,
    ambientLight: null,
    timeOfDay: 12, // 0 to 24
    carsData: [],
    
    init: function (containerId) {
        const container = document.getElementById(containerId);
        if (!container) return;

        // Clean up previous instance if exists
        if (this.renderer) {
            container.innerHTML = '';
        }

        // Scene
        this.scene = new THREE.Scene();
        this.scene.background = new THREE.Color(0x87ceeb); // Day sky by default
        this.scene.fog = new THREE.FogExp2(0x87ceeb, 0.002);

        // Camera
        this.camera = new THREE.PerspectiveCamera(60, container.clientWidth / container.clientHeight, 1, 10000);
        this.camera.position.set(100, 150, 200);

        // Renderer
        this.renderer = new THREE.WebGLRenderer({ antialias: true });
        this.renderer.setSize(container.clientWidth, container.clientHeight);
        this.renderer.shadowMap.enabled = true;
        this.renderer.shadowMap.type = THREE.PCFSoftShadowMap;
        container.appendChild(this.renderer.domElement);

        // Controls
        this.controls = new THREE.OrbitControls(this.camera, this.renderer.domElement);
        this.controls.enableDamping = true;
        this.controls.dampingFactor = 0.05;
        this.controls.maxPolarAngle = Math.PI / 2 - 0.05; // Don't go below ground
        this.controls.minDistance = 20;
        this.controls.maxDistance = 1000;

        // Lights
        this.ambientLight = new THREE.AmbientLight(0x404040, 1); // Soft light
        this.scene.add(this.ambientLight);

        this.directionalLight = new THREE.DirectionalLight(0xffffff, 2);
        this.directionalLight.position.set(200, 500, 300);
        this.directionalLight.castShadow = true;
        this.directionalLight.shadow.camera.left = -500;
        this.directionalLight.shadow.camera.right = 500;
        this.directionalLight.shadow.camera.top = 500;
        this.directionalLight.shadow.camera.bottom = -500;
        this.directionalLight.shadow.camera.far = 2000;
        this.directionalLight.shadow.mapSize.width = 2048;
        this.directionalLight.shadow.mapSize.height = 2048;
        this.scene.add(this.directionalLight);

        // Ground
        const groundGeo = new THREE.PlaneGeometry(2000, 2000);
        const groundMat = new THREE.MeshStandardMaterial({ color: 0x3d7c47, roughness: 0.8, metalness: 0.2 });
        const ground = new THREE.Mesh(groundGeo, groundMat);
        ground.rotation.x = -Math.PI / 2;
        ground.receiveShadow = true;
        this.scene.add(ground);

        // Build City Environment
        this.generateRoads();
        this.generateCity();
        this.generateCars();
        this.generateWeather();

        // Resize handler
        window.addEventListener('resize', this.onResize.bind(this));

        // Start Loop
        this.animate();
    },

    generateRoads: function() {
        const roadMat = new THREE.MeshStandardMaterial({ color: 0x333333, roughness: 0.9 });
        const roadGeomV = new THREE.PlaneGeometry(20, 2000);
        const roadGeomH = new THREE.PlaneGeometry(2000, 20);

        for (let i = -800; i <= 800; i += 200) {
            const vRoad = new THREE.Mesh(roadGeomV, roadMat);
            vRoad.rotation.x = -Math.PI / 2;
            vRoad.position.set(i, 0.5, 0);
            vRoad.receiveShadow = true;
            this.scene.add(vRoad);

            const hRoad = new THREE.Mesh(roadGeomH, roadMat);
            hRoad.rotation.x = -Math.PI / 2;
            hRoad.position.set(0, 0.5, i);
            hRoad.receiveShadow = true;
            this.scene.add(hRoad);
        }
    },

    generateCity: function () {
        const buildingCount = 2000;
        const boxGeo = new THREE.BoxGeometry(1, 1, 1);
        const material = new THREE.MeshStandardMaterial({ color: 0xffffff, roughness: 0.5 });
        
        this.buildingsMesh = new THREE.InstancedMesh(boxGeo, material, buildingCount);
        this.buildingsMesh.castShadow = true;
        this.buildingsMesh.receiveShadow = true;

        const dummy = new THREE.Object3D();
        const color = new THREE.Color();

        for (let i = 0; i < buildingCount; i++) {
            let x = (Math.random() - 0.5) * 1600;
            let z = (Math.random() - 0.5) * 1600;

            // Avoid placing buildings exactly on roads (every 200 units is a road)
            const snapX = Math.round(x / 200) * 200;
            const snapZ = Math.round(z / 200) * 200;
            
            if (Math.abs(x - snapX) < 20) x += (x > snapX ? 20 : -20);
            if (Math.abs(z - snapZ) < 20) z += (z > snapZ ? 20 : -20);

            // Distance from center dictates height (Downtown)
            const dist = Math.sqrt(x * x + z * z);
            const maxH = Math.max(10, 200 - (dist * 0.2));
            const h = Math.random() * maxH + 10;
            const w = Math.random() * 20 + 10;
            const d = Math.random() * 20 + 10;

            dummy.position.set(x, h / 2, z);
            dummy.scale.set(w, h, d);
            dummy.updateMatrix();

            this.buildingsMesh.setMatrixAt(i, dummy.matrix);

            // Zone colors: Residential (greenish), Commercial (blueish), Industrial (gray/brownish)
            const zoneType = Math.random();
            if (zoneType < 0.5) {
                color.setHex(0x55aa55); // Res
            } else if (zoneType < 0.8) {
                color.setHex(0x5555aa); // Com
            } else {
                color.setHex(0xaaaaaa); // Ind
            }
            
            // Random variation
            color.offsetHSL(0, 0, (Math.random() - 0.5) * 0.2);
            this.buildingsMesh.setColorAt(i, color);
        }
        
        this.buildingsMesh.instanceMatrix.needsUpdate = true;
        if(this.buildingsMesh.instanceColor) this.buildingsMesh.instanceColor.needsUpdate = true;
        this.scene.add(this.buildingsMesh);
    },

    generateCars: function() {
        const carCount = 300;
        const carGeo = new THREE.BoxGeometry(4, 2, 8);
        const carMat = new THREE.MeshStandardMaterial({ color: 0xff3333, roughness: 0.4 });
        
        this.carsMesh = new THREE.InstancedMesh(carGeo, carMat, carCount);
        this.carsMesh.castShadow = true;

        const dummy = new THREE.Object3D();
        const color = new THREE.Color();

        for (let i = 0; i < carCount; i++) {
            const isHorizontal = Math.random() > 0.5;
            const lane = (Math.round((Math.random() - 0.5) * 8) * 200); 
            
            let x, z;
            if(isHorizontal) {
                x = (Math.random() - 0.5) * 1600;
                z = lane + (Math.random() > 0.5 ? 5 : -5);
            } else {
                x = lane + (Math.random() > 0.5 ? 5 : -5);
                z = (Math.random() - 0.5) * 1600;
            }

            const speed = (Math.random() * 1.5 + 0.5) * (Math.random() > 0.5 ? 1 : -1);

            this.carsData.push({
                x: x, z: z, 
                isHorizontal: isHorizontal, 
                speed: speed
            });

            dummy.position.set(x, 1.5, z);
            if(isHorizontal) {
                dummy.rotation.y = Math.PI / 2;
            }
            dummy.updateMatrix();
            this.carsMesh.setMatrixAt(i, dummy.matrix);

            color.setHex(Math.random() * 0xffffff);
            this.carsMesh.setColorAt(i, color);
        }

        this.carsMesh.instanceMatrix.needsUpdate = true;
        if(this.carsMesh.instanceColor) this.carsMesh.instanceColor.needsUpdate = true;
        this.scene.add(this.carsMesh);
    },

    generateWeather: function() {
        const rainCount = 10000;
        const rainGeo = new THREE.BufferGeometry();
        const rainVerts = [];
        
        for(let i=0; i<rainCount; i++) {
            rainVerts.push(
                Math.random() * 2000 - 1000,
                Math.random() * 500,
                Math.random() * 2000 - 1000
            );
        }
        
        rainGeo.setAttribute('position', new THREE.Float32BufferAttribute(rainVerts, 3));
        const rainMat = new THREE.PointsMaterial({
            color: 0xaaaaaa,
            size: 0.5,
            transparent: true,
            opacity: 0.6
        });
        
        this.rainParticles = new THREE.Points(rainGeo, rainMat);
        this.rainParticles.visible = false; // off by default
        this.scene.add(this.rainParticles);
    },

    updateTime: function (hour) {
        this.timeOfDay = hour;
        // Map 0-24 to Sun Position
        // 6AM sunrise, 18PM sunset
        const timeFraction = (this.timeOfDay - 6) / 24; 
        const angle = timeFraction * Math.PI * 2;
        
        const radius = 800;
        this.directionalLight.position.x = Math.cos(angle) * radius;
        this.directionalLight.position.y = Math.sin(angle) * radius;
        
        // Night Time coloring
        if (this.timeOfDay > 18 || this.timeOfDay < 6) {
            this.scene.background.setHex(0x050510);
            this.scene.fog.color.setHex(0x050510);
            this.ambientLight.intensity = 0.2;
            this.directionalLight.intensity = 0.1;
        } else {
            // Day Time
            const t = Math.sin((this.timeOfDay - 6) / 12 * Math.PI);
            
            // Sunrise/sunset colors
            if (this.timeOfDay < 8 || this.timeOfDay > 16) {
                this.scene.background.setHex(0xff7e00); // orange
                this.scene.fog.color.setHex(0xff7e00);
            } else {
                this.scene.background.setHex(0x87ceeb); // sky blue
                this.scene.fog.color.setHex(0x87ceeb);
            }
            
            this.ambientLight.intensity = 0.6 + t * 0.4;
            this.directionalLight.intensity = 1.0 + t * 1.0;
        }
    },

    setWeather: function(weatherStr) {
        if(weatherStr === "Rain") {
            this.rainParticles.visible = true;
            this.scene.fog.density = 0.005; // thicker fog
        } else if (weatherStr === "Fog") {
            this.rainParticles.visible = false;
            this.scene.fog.density = 0.01;
        } else {
            // Clear
            this.rainParticles.visible = false;
            this.scene.fog.density = 0.002;
        }
    },

    animate: function () {
        this.animationId = requestAnimationFrame(this.animate.bind(this));
        
        this.controls.update();

        // Animate Cars
        const dummy = new THREE.Object3D();
        for(let i=0; i<this.carsData.length; i++) {
            let car = this.carsData[i];
            
            if(car.isHorizontal) {
                car.x += car.speed;
                if(car.x > 1000) car.x = -1000;
                if(car.x < -1000) car.x = 1000;
            } else {
                car.z += car.speed;
                if(car.z > 1000) car.z = -1000;
                if(car.z < -1000) car.z = 1000;
            }
            
            dummy.position.set(car.x, 1.5, car.z);
            if(car.isHorizontal) dummy.rotation.y = Math.PI / 2;
            else dummy.rotation.y = 0;
            dummy.updateMatrix();
            this.carsMesh.setMatrixAt(i, dummy.matrix);
        }
        this.carsMesh.instanceMatrix.needsUpdate = true;

        // Animate Rain
        if(this.rainParticles.visible) {
            const positions = this.rainParticles.geometry.attributes.position.array;
            for(let i=1; i<positions.length; i+=3) {
                positions[i] -= 5; // fall down
                if(positions[i] < 0) {
                    positions[i] = 500;
                }
            }
            this.rainParticles.geometry.attributes.position.needsUpdate = true;
        }

        this.renderer.render(this.scene, this.camera);
    },

    onResize: function () {
        const container = this.renderer.domElement.parentElement;
        if (container) {
            this.camera.aspect = container.clientWidth / container.clientHeight;
            this.camera.updateProjectionMatrix();
            this.renderer.setSize(container.clientWidth, container.clientHeight);
        }
    },

    dispose: function() {
        if (this.animationId) {
            cancelAnimationFrame(this.animationId);
        }
        window.removeEventListener('resize', this.onResize);
    }
};
