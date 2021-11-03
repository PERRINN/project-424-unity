clc;
clear;

%% Load data
% subplot_title = 'Inertia Test 5000 X 1000 Z';
% x_acceleration_simdrive_path = 'Inertia Test 5000_X_1000Z - Simdrive- angular  acceleration X.txt'; 
% y_acceleration_simdrive_path = 'Inertia Test 5000_X_1000Z - Simdrive- angular  acceleration Y.txt'; 
% z_acceleration_simdrive_path = 'Inertia Test 5000_X_1000Z - Simdrive- angular  acceleration Z.txt'; 
% results_path = 'Inertia Test 5000_X_1000Z - Results.txt';

subplot_title = 'Inertia Test 5000 Y 1000 Z';
x_acceleration_simdrive_path = 'Inertia Test 5000_Y_1000Z - Simdrive- angular  acceleration X.txt'; 
y_acceleration_simdrive_path = 'Inertia Test 5000_Y_1000Z - Simdrive- angular  acceleration Y.txt'; 
z_acceleration_simdrive_path = 'Inertia Test 5000_Y_1000Z - Simdrive- angular  acceleration Z.txt'; 
results_path = 'Inertia Test 5000_Y_1000Z - Results.txt'; 

x_simdrive_file = importdata(x_acceleration_simdrive_path);
y_simdrive_file = importdata(y_acceleration_simdrive_path);
z_simdrive_file = importdata(z_acceleration_simdrive_path);
results_file = importdata(results_path);

results = results_file.data;
max_time = max(results(:,1));

x_simdrive = x_simdrive_file.data;
y_simdrive = y_simdrive_file.data;
z_simdrive = z_simdrive_file.data;

x_simdrive = clamp(max_time, x_simdrive);
y_simdrive = clamp(max_time, y_simdrive);
z_simdrive = clamp(max_time, z_simdrive);

%% Calculate error
[x_r, x_RMSE] = analysis(x_simdrive, results(:,[1 2]));
[y_r, y_RMSE] = analysis(y_simdrive, results(:,[1 3]));
[z_r, z_RMSE] = analysis(z_simdrive, results(:,[1 4]));

%% Plot
formatSpec = '%s\nCorrelation: %f. RMSE: %e\n';
subplot(3,1,1);
plot(x_simdrive(:,1), x_simdrive(:,2));
t = sprintf(formatSpec, 'Angular Acceleration X axis', x_r, x_RMSE);
title(t);
disp(t);
hold on
plot(results(:,1), results(:,2));
legend('sim drive','unity')
hold off
subplot(3,1,2);
plot(y_simdrive(:,1), y_simdrive(:,2));
t = sprintf(formatSpec, 'Angular Acceleration Y axis', y_r, y_RMSE);
title(t);
disp(t);
hold on
plot(results(:,1), results(:,3));
legend('sim drive','unity')
hold off
subplot(3,1,3);
plot(z_simdrive(:,1), z_simdrive(:,2));
t = sprintf(formatSpec, 'Angular Acceleration Z axis', z_r, z_RMSE);
title(t);
disp(t);hold on
plot(results(:,1), results(:,4));
legend('sim drive','unity')
hold off
sgt = sgtitle(subplot_title);

%% Functions
function clamped = clamp(max, raw_values)
    indixes = find(raw_values(:,1) > max);
    first_index = indixes(1);
    clamped = raw_values(1:first_index, :);
end

function fixed = fix_sample_points(reference,to_be_fixed)
    t1 = reference(:,1);
    y1 = reference(:,2);
    t2 = to_be_fixed(:,1);
    y2 = to_be_fixed(:,2);
    y2i = interp1(t2, y2, t1, 'PCHIP');
    fixed = [t1, y2i];
end

function [R, RMSE] = analysis(reference,simulated)  
    simulated = fix_sample_points(reference, simulated);
    y_reference =  reference(:,2);
    y_simulated = simulated(:,2);
    R = corr(y_reference,y_simulated,'Type', 'Pearson');
    
    dy = y_reference-y_simulated ; % error 
    abs_dy = abs(y_reference-y_simulated) ;   % absolute error 
    relerr = abs(y_reference-y_simulated)./y_reference ;  % relative error 
    pererr = abs(y_reference-y_simulated)./y_reference*100 ;   % percentage error 
    mean_err = mean(abs(y_reference-y_simulated)) ;    % mean absolute error 
    MSE = mean((y_reference-y_simulated).^2) ;        % Mean square error 
    RMSE = sqrt(mean((y_reference-y_simulated).^2)) ; % Root mean square error 
end