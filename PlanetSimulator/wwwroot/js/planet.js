// Global variables
window.planetSimulator = {
    scene: null,
    camera: null,
    renderer: null,
    raycaster: null,
    mouse: null,
    dotNetRef: null,
    controls: null,
    
    // Animation & Logic
    celestialBodies: [],
    orbits: [],
    animationId: null,
    simulationSpeed: 0.2,
    isPaused: false,

    // Initialize
    init: function (canvasId, dotNetReference) {
        this.dotNetRef = dotNetReference;
        const container = document.getElementById(canvasId);
        if (!container) return;

        // Cleanup
        this.cleanup();

        // Scene setup
        this.scene = new THREE.Scene();
        this.scene.background = new THREE.Color(0x000005); // Deep space black

        // Camera
        this.camera = new THREE.PerspectiveCamera(45, container.clientWidth / container.clientHeight, 0.1, 20000);
        this.camera.position.set(0, 60, 150);
        this.camera.lookAt(0, 0, 0);

        // Renderer
        this.renderer = new THREE.WebGLRenderer({ antialias: true });
        this.renderer.setSize(container.clientWidth, container.clientHeight);
        this.renderer.shadowMap.enabled = true;
        this.renderer.shadowMap.type = THREE.PCFSoftShadowMap;
        container.appendChild(this.renderer.domElement);

        // Controls (OrbitControls)
        if (THREE.OrbitControls) {
            this.controls = new THREE.OrbitControls(this.camera, this.renderer.domElement);
            this.controls.enableDamping = true;
            this.controls.dampingFactor = 0.05;
            this.controls.minDistance = 10;
            this.controls.maxDistance = 1000;
        }

        // Raycaster for clicks
        this.raycaster = new THREE.Raycaster();
        this.mouse = new THREE.Vector2();

        // Lighting
        const ambientLight = new THREE.AmbientLight(0x333333); 
        this.scene.add(ambientLight);

        // Sun Light (Point Light at center)
        const sunLight = new THREE.PointLight(0xffffff, 2, 800);
        sunLight.position.set(0, 0, 0);
        sunLight.castShadow = true;
        sunLight.shadow.mapSize.width = 2048;
        sunLight.shadow.mapSize.height = 2048;
        this.scene.add(sunLight);

        // Create Solar System
        this.createSolarSystem();

        // Create Starfield
        this.createStars();

        // Events
        window.addEventListener('resize', this.onWindowResize.bind(this), false);
        this.renderer.domElement.addEventListener('pointerdown', this.onMouseDown.bind(this), false);

        // Start Loop
        this.animate();
    },

    cleanup: function() {
        if (this.animationId) cancelAnimationFrame(this.animationId);
        const container = this.renderer ? this.renderer.domElement.parentNode : null;
        if (container) container.innerHTML = '';
        this.celestialBodies = [];
        this.orbits = [];
        this.controls = null;
    },

    createSolarSystem: function() {
        // Data: Size relative to visual scale (not real), Distance, Color, Speed, Name, Desc
        
        // 1. Sun
        const sun = this.createBody(null, 8, 0, 0xffcc00, 0, 0.005, {
            name: "Sun",
            type: "Star",
            description: "The star at the center of the Solar System. It is a nearly perfect sphere of hot plasma."
        }, true, null); // No texture for Sun yet (unless added later)

        // 2. Mercury
        this.createBody(sun.mesh, 1.0, 15, 0xaaaaaa, 0.02, 0.01, {
            name: "Mercury",
            type: "Planet",
            description: "The smallest planet in the Solar System and the closest to the Sun."
        });

        // 3. Venus
        this.createBody(sun.mesh, 1.8, 22, 0xe3bb76, 0.015, 0.005, {
            name: "Venus",
            type: "Planet",
            description: "The second planet from the Sun. It has a dense atmosphere of carbon dioxide."
        });

        // 4. Earth
        const earth = this.createBody(sun.mesh, 2, 32, 0x2233ff, 0.01, 0.02, {
            name: "Earth",
            type: "Planet",
            description: "Our home. The only astronomical object known to harbor life."
        }, false, 'textures/earth.jpg');
        
        // Moon
        this.createBody(earth.mesh, 0.5, 4, 0xdddddd, 0.05, 0.01, {
            name: "Moon",
            type: "Satellite",
            description: "Earth's only natural satellite."
        }, false, 'textures/moon.jpg');

        // 5. Mars
        const mars = this.createBody(sun.mesh, 1.6, 42, 0xc1440e, 0.008, 0.015, {
            name: "Mars",
            type: "Planet",
            description: "The Red Planet. Dusty, cold, desert world with a very thin atmosphere."
        });

        // 6. Jupiter
        const jupiter = this.createBody(sun.mesh, 5, 65, 0xd8ca9d, 0.004, 0.04, {
            name: "Jupiter",
            type: "Planet",
            description: "The largest planet in the Solar System. A gas giant with a Great Red Spot."
        });
        // Europa (Moon)
        this.createBody(jupiter.mesh, 0.6, 7, 0xaaccff, 0.03, 0.01, {name: "Europa", type: "Satellite", description: "Jupiter's moon, likely has a subsurface ocean."});

        // 7. Saturn
        const saturn = this.createBody(sun.mesh, 4.5, 90, 0xcfa575, 0.003, 0.035, {
            name: "Saturn",
            type: "Planet",
            description: "Adorned with a dazzling, complex system of icy rings."
        });
        this.createRings(saturn.mesh, 6, 9, 0xcfa575);

        // 8. Uranus
        this.createBody(sun.mesh, 3, 115, 0x93b8be, 0.002, 0.02, {
            name: "Uranus",
            type: "Planet",
            description: "An ice giant. It rotates at a nearly 90-degree angle from the plane of its orbit."
        });

        // 9. Neptune
        this.createBody(sun.mesh, 2.9, 135, 0x2d4399, 0.0015, 0.02, {
            name: "Neptune",
            type: "Planet",
            description: "The most distant major planet. Dark, cold, and whipped by supersonic winds."
        });
    },

    createBody: function(parentMesh, size, distance, color, orbitSpeed, rotationSpeed, info, isEmissive = false, texturePath = null) {
        // Geometry
        const geometry = new THREE.SphereGeometry(size, 64, 64);
        
        // Material
        let material;
        const textureLoader = new THREE.TextureLoader();

        if (texturePath) {
            const texture = textureLoader.load(texturePath);
            if (isEmissive) {
                material = new THREE.MeshBasicMaterial({ map: texture, color: 0xffffff });
            } else {
                material = new THREE.MeshStandardMaterial({ 
                    map: texture, 
                    roughness: 0.8, 
                    metalness: 0.2 
                });
            }
        } else {
            if (isEmissive) {
                material = new THREE.MeshBasicMaterial({ color: color });
            } else {
                material = new THREE.MeshStandardMaterial({ 
                    color: color, 
                    roughness: 0.7, 
                    metalness: 0.1 
                });
            }
        }

        const mesh = new THREE.Mesh(geometry, material);
        
        // Shadow
        if(!isEmissive) {
            mesh.castShadow = true;
            mesh.receiveShadow = true;
        }

        // Add to scene
        this.scene.add(mesh);

        // Orbit Line
        if (parentMesh) {
            const orbitCurve = new THREE.EllipseCurve(
                0, 0,            
                distance, distance, 
                0, 2 * Math.PI,  
                false,            
                0                 
            );
            const points = orbitCurve.getPoints(128);
            const orbitGeo = new THREE.BufferGeometry().setFromPoints(points);
            const orbitMat = new THREE.LineBasicMaterial({ color: 0x555555, transparent: true, opacity: 0.2 });
            const orbitLine = new THREE.Line(orbitGeo, orbitMat);
            orbitLine.rotation.x = Math.PI / 2;
            
            if(parentMesh.position.x === 0 && parentMesh.position.z === 0) {
                 this.scene.add(orbitLine); 
            } else {
                parentMesh.add(orbitLine);
            }
        }

        // Store data
        const body = {
            mesh: mesh,
            parent: parentMesh,
            distance: distance,
            angle: Math.random() * Math.PI * 2,
            orbitSpeed: orbitSpeed,
            rotationSpeed: rotationSpeed,
            info: info
        };

        this.celestialBodies.push(body);
        return body;
    },

    createRings: function(parentMesh, innerRadius, outerRadius, color) {
        const geometry = new THREE.RingGeometry(innerRadius, outerRadius, 64);
        const pos = geometry.attributes.position;
        const v3 = new THREE.Vector3();
        for (let i = 0; i < pos.count; i++){
            v3.fromBufferAttribute(pos, i);
            geometry.attributes.uv.setXY(i, v3.length() < (innerRadius+outerRadius)/2 ? 0 : 1, 1);
        }
        
        const material = new THREE.MeshStandardMaterial({ 
            color: color, 
            side: THREE.DoubleSide,
            transparent: true,
            opacity: 0.6
        });
        
        const rings = new THREE.Mesh(geometry, material);
        rings.rotation.x = Math.PI / 2;
        parentMesh.add(rings);
    },

    createStars: function() {
        const geometry = new THREE.BufferGeometry();
        const vertices = [];
        for (let i = 0; i < 8000; i++) {
            vertices.push(THREE.MathUtils.randFloatSpread(2000));
            vertices.push(THREE.MathUtils.randFloatSpread(2000));
            vertices.push(THREE.MathUtils.randFloatSpread(2000));
        }
        geometry.setAttribute('position', new THREE.Float32BufferAttribute(vertices, 3));
        const stars = new THREE.Points(geometry, new THREE.PointsMaterial({ color: 0xffffff, size: 0.7 }));
        this.scene.add(stars);
    },

    animate: function() {
        this.animationId = requestAnimationFrame(this.animate.bind(this));
        
        if (this.controls) this.controls.update();

        if (this.isPaused) {
            this.renderer.render(this.scene, this.camera);
            return;
        }

        // Update positions
        this.celestialBodies.forEach(body => {
            body.mesh.rotation.y += body.rotationSpeed;

            if (body.parent) {
                body.angle += body.orbitSpeed * this.simulationSpeed;
                
                const x = Math.cos(body.angle) * body.distance;
                const z = Math.sin(body.angle) * body.distance;
                
                const parentPos = body.parent.position;
                body.mesh.position.set(parentPos.x + x, parentPos.y, parentPos.z + z);
            }
        });

        this.renderer.render(this.scene, this.camera);
    },

    onWindowResize: function() {
        if (!this.camera || !this.renderer) return;
        const container = this.renderer.domElement.parentNode;
        this.camera.aspect = container.clientWidth / container.clientHeight;
        this.camera.updateProjectionMatrix();
        this.renderer.setSize(container.clientWidth, container.clientHeight);
    },

    onMouseDown: function(event) {
        // Only trigger selection if we are not dragging (simple check)
        // But dragging is handled by OrbitControls, which swallows events if enabled.
        // We can check if mouse moved significantly or use 'click' event.
        // For simplicity, we use pointerdown but we need to check intersection.

        const rect = this.renderer.domElement.getBoundingClientRect();
        this.mouse.x = ((event.clientX - rect.left) / rect.width) * 2 - 1;
        this.mouse.y = -((event.clientY - rect.top) / rect.height) * 2 + 1;

        this.raycaster.setFromCamera(this.mouse, this.camera);

        const objectsToCheck = this.celestialBodies.map(b => b.mesh);
        const intersects = this.raycaster.intersectObjects(objectsToCheck);

        if (intersects.length > 0) {
            const selectedMesh = intersects[0].object;
            const body = this.celestialBodies.find(b => b.mesh === selectedMesh);
            if (body && this.dotNetRef) {
                this.dotNetRef.invokeMethodAsync('SetSelectedObject', body.info);
            }
        }
    },

    setSpeed: function(speed) {
        this.simulationSpeed = speed;
    },

    // UI Control Helper Methods
    zoomIn: function() {
        // Move camera closer to target
        const direction = new THREE.Vector3();
        this.camera.getWorldDirection(direction);
        this.camera.position.addScaledVector(direction, 20); 
    },

    zoomOut: function() {
        const direction = new THREE.Vector3();
        this.camera.getWorldDirection(direction);
        this.camera.position.addScaledVector(direction, -20);
    },

    rotateLeft: function() {
        // Rotate around 0,0,0 (Sun)
        // Simplified: just rotate camera position around origin
        const x = this.camera.position.x;
        const z = this.camera.position.z;
        const speed = 0.1;
        this.camera.position.x = x * Math.cos(speed) - z * Math.sin(speed);
        this.camera.position.z = x * Math.sin(speed) + z * Math.cos(speed);
        this.camera.lookAt(0,0,0);
    },

    rotateRight: function() {
        const x = this.camera.position.x;
        const z = this.camera.position.z;
        const speed = -0.1;
        this.camera.position.x = x * Math.cos(speed) - z * Math.sin(speed);
        this.camera.position.z = x * Math.sin(speed) + z * Math.cos(speed);
        this.camera.lookAt(0,0,0);
    }
};
