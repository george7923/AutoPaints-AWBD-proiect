document.addEventListener("DOMContentLoaded", function () {
  const containerCarrossel = document.querySelector(".container-carrossel");
  if (!containerCarrossel) {
    console.error("Container carrossel not found");
    return;
  }
  console.log("Container carrossel found:", containerCarrossel);

  const carrossel = document.querySelector(".carrossel");
  if (!carrossel) {
    console.error("Carrossel not found");
    return;
  }
  console.log("Carrossel found:", carrossel);

  const carrosselItems = document.querySelectorAll(".carrossel-item");
  if (!carrosselItems.length) {
    console.error("No carrossel items found");
    return;
  }
  console.log("Carrossel items found:", carrosselItems);

  // Iniciamos variables que cambiaran su estado.
  let isMouseDown = false;
  let currentMousePos = 0;
  let lastMousePos = 0;
  let lastMoveTo = 0;
  let moveTo = 0;

  const createCarrossel = () => {
    const carrosselProps = onResize();
    const length = carrosselItems.length;
    const degrees = 360 / length;
    const gap = 20;
    const tz = distanceZ(carrosselProps.w, length, gap);

    carrosselItems.forEach((item, i) => {
      const degreesByItem = degrees * i + "deg";
      item.style.setProperty("--rotatey", degreesByItem);
      item.style.setProperty("--tz", tz + "px");
    });
  };

  const lerp = (a, b, n) => {
    return n * (a - b) + b;
  };

  const distanceZ = (widthElement, length, gap) => {
    return widthElement / 2 / Math.tan(Math.PI / length) + gap;
  };

  const getPosX = (x) => {
    currentMousePos = x;
    moveTo = currentMousePos < lastMousePos ? moveTo - 2 : moveTo + 2;
    lastMousePos = currentMousePos;
  };

  const update = () => {
    if (isMouseDown) {
      lastMoveTo = lerp(moveTo, lastMoveTo, 0.05);
      carrossel.style.setProperty("--rotatey", lastMoveTo + "deg");
    }
    requestAnimationFrame(update);
  };

  const onResize = () => {
    const boundingCarrossel = containerCarrossel.getBoundingClientRect();
    const carrosselProps = {
      w: boundingCarrossel.width,
      h: boundingCarrossel.height,
    };
    return carrosselProps;
  };

  const initEvents = () => {
    carrossel.addEventListener("mousedown", () => {
      isMouseDown = true;
      carrossel.style.cursor = "grabbing";
    });

    carrossel.addEventListener("mouseup", () => {
      isMouseDown = false;
      carrossel.style.cursor = "grab";
    });

    containerCarrossel.addEventListener("mouseleave", () => {
      isMouseDown = false;
    });

    carrossel.addEventListener("mousemove", (e) => {
      if (isMouseDown) {
        getPosX(e.clientX);
      }
    });

    carrossel.addEventListener("touchstart", () => {
      isMouseDown = true;
      carrossel.style.cursor = "grabbing";
    });

    carrossel.addEventListener("touchend", () => {
      isMouseDown = false;
      carrossel.style.cursor = "grab";
    });

    containerCarrossel.addEventListener("touchmove", (e) => {
      if (isMouseDown) {
        getPosX(e.touches[0].clientX);
      }
    });

    window.addEventListener("resize", createCarrossel);

    update();
    createCarrossel();
  };

  initEvents();
});
