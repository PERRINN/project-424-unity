clc;
clear;

x_simdrive_file = importdata('Inertia Test 02 - Simdrive - angular acceleration X.txt');
y_simdrive_file = importdata('Inertia Test 02 - Simdrive - angular acceleration Y.txt');
z_simdrive_file = importdata('Inertia Test 02 - Simdrive - angular acceleration Z.txt');
results_file = importdata('Inertia Test 02 - Results.txt');

results = results_file.data;
max_time = max(results(:,1));

x_simdrive = x_simdrive_file.data;
y_simdrive = y_simdrive_file.data;
z_simdrive = z_simdrive_file.data;

x_simdrive = clamp(max_time, x_simdrive);
y_simdrive = clamp(max_time, y_simdrive);
z_simdrive = clamp(max_time, z_simdrive);


subplot(3,1,1);
plot(x_simdrive(:,1), x_simdrive(:,2));
hold on
plot(results(:,1), results(:,2));
hold off
subplot(3,1,2);
plot(y_simdrive(:,1), y_simdrive(:,2));
hold on
plot(results(:,1), results(:,3));
hold off
subplot(3,1,3);
plot(z_simdrive(:,1), z_simdrive(:,2));
hold on
plot(results(:,1), results(:,4));
hold off

[x_r, x_RMSE] = analysis(x_simdrive, results(:,[1 2]), 'Pearson');
[y_r, y_RMSE] = analysis(y_simdrive, results(:,[1 3]), 'Pearson');
[z_r, z_RMSE] = analysis(z_simdrive, results(:,[1 4]), 'Pearson');

formatSpec = 'Axis %s => Correlation: %f. RMSE: %e\n';
fprintf(formatSpec,"x",x_r,x_RMSE);
fprintf(formatSpec,"y",y_r,y_RMSE);
fprintf(formatSpec,"z",z_r,z_RMSE);


function clamped = clamp(max, raw_values)
    indixes = find(raw_values(:,1) > max);
    first_index = indixes(1);
    clamped = raw_values(1:first_index, :);
end

function [R, RMSE] = analysis(A,B, type)
    t1 = A(:,1);
    y1 = A(:,2);
    t2 = B(:,1);
    y2 = B(:,2);
    y2i = interp1(t2, y2, t1, 'PCHIP');
    %R = corrcoef(y1, y2i);
    %R = corrcoef(y1, y2i);
    R = corr(y1,y2i,'Type', type);
    
    y0 = y1;
    y1 = y2i;
    dy = y0-y1 ; % error 
    abs_dy = abs(y0-y1) ;   % absolute error 
    relerr = abs(y0-y1)./y0 ;  % relative error 
    pererr = abs(y0-y1)./y0*100 ;   % percentage error 
    mean_err = mean(abs(y0-y1)) ;    % mean absolute error 
    MSE = mean((y0-y1).^2) ;        % Mean square error 
    RMSE = sqrt(mean((y0-y1).^2)) ; % Root mean square error 
end